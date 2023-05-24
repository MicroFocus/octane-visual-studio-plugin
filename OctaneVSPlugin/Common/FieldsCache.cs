/*!
* Copyright 2017 – 2023 Open Text.
*
* The only warranties for products and services of Open Text and
* its affiliates and licensors (“Open Text”) are as may be set
* forth in the express warranty statements accompanying such products
* and services. Nothing herein should be construed as constituting an
* additional warranty. Open Text shall not be liable for technical or
* editorial errors or omissions contained herein. The information
* contained herein is subject to change without notice.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using MicroFocus.Adm.Octane.Api.Core.Entities;
using MicroFocus.Adm.Octane.VisualStudio.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace MicroFocus.Adm.Octane.VisualStudio.Common
{
    /// <summary>
    /// Cache responsible for maintaining the default fields for each entity type
    /// </summary>
    public class FieldsCache
    {
        private static FieldsCache _fieldsCache;
        private static readonly object _obj = new object();

        private static Metadata _defaultFieldsCache = null;
        private static Metadata _persistedFieldsCache = null;

        private FieldsCache()
        {
            DeserializeDefaultFieldsMetadata();
            DeserializePersistedFieldsMetadata();
        }

        public static FieldsCache Instance
        {
            get
            {
                if (_fieldsCache == null)
                {
                    lock (_obj)
                    {
                        if (_fieldsCache == null)
                        {
                            _fieldsCache = new FieldsCache();
                        }
                    }
                }

                return _fieldsCache;
            }
        }

        private static void DeserializeDefaultFieldsMetadata()
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream("MicroFocus.Adm.Octane.VisualStudio.Resources.DefaultFields.json"))
            using (StreamReader reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();
                _defaultFieldsCache = Utility.DeserializeFromJson(result, new Metadata
                {
                    data = new Dictionary<string, HashSet<string>>(),
                    version = 1
                });
            }
        }

        private static void DeserializePersistedFieldsMetadata()
        {
            var persistedJson = OctanePluginSettings.Default.EntityFields;
            _persistedFieldsCache = Utility.DeserializeFromJson(persistedJson, new Metadata
            {
                data = new Dictionary<string, HashSet<string>>(),
                version = 1
            });
        }

        private static void PersistFieldsMetadata()
        {
            try
            {
                OctanePluginSettings.Default.EntityFields = Utility.SerializeToJson(_persistedFieldsCache);
                OctanePluginSettings.Default.Save();
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Check whether the input fields are the same as the default fields for the given entity type
        /// </summary>
        public bool AreSameFieldsAsDefaultFields(string entityType, List<FieldViewModel> fields)
        {
            if (string.IsNullOrEmpty(entityType) || fields == null)
                return false;

            HashSet<string> defaultVisibleFields;
            if (!_defaultFieldsCache.data.TryGetValue(entityType, out defaultVisibleFields))
                return false;

            if (fields.Count != defaultVisibleFields.Count)
                return false;

            return fields.All(f => defaultVisibleFields.Contains(f.Name) && f.IsSelected);
        }

        /// <summary>
        /// Returns all the visible fields for the given entity type
        /// </summary>
        public HashSet<string> GetVisibleFieldsForEntity(string entityType)
        {
            var emptyHashSet = new HashSet<string>();
            if (string.IsNullOrEmpty(entityType))
                return emptyHashSet;

            HashSet<string> visibleFields;
            return !_persistedFieldsCache.data.TryGetValue(entityType, out visibleFields)
                ? !_defaultFieldsCache.data.TryGetValue(entityType, out visibleFields)
                    ? emptyHashSet
                    : visibleFields
                : visibleFields;
        }

        /// <summary>
        /// Updates the cache with the current visible fields for the given entity type
        /// </summary>
        public void UpdateVisibleFieldsForEntity(string entityType, List<FieldViewModel> allFields)
        {
            if (string.IsNullOrEmpty(entityType) || allFields == null)
                return;

            HashSet<string> visibleFields;
            if (!_persistedFieldsCache.data.TryGetValue(entityType, out visibleFields))
            {
                visibleFields = new HashSet<string>();
                _persistedFieldsCache.data[entityType] = visibleFields;
            }

            visibleFields.Clear();

            foreach (var field in allFields.Where(f => f.IsSelected))
            {
                visibleFields.Add(field.Name);
            }

            PersistFieldsMetadata();
            Notify(entityType);
        }

        /// <summary>
        /// Reset to the default visible fields for the given entity type
        /// </summary>
        public void ResetVisibleFieldsForEntity(string entityType)
        {
            if (string.IsNullOrEmpty(entityType))
                return;

            if (!_persistedFieldsCache.data.ContainsKey(entityType))
                return;

            HashSet<string> defaultVisibleFields;
            if (!_defaultFieldsCache.data.TryGetValue(entityType, out defaultVisibleFields))
                return;

            _persistedFieldsCache.data[entityType] = new HashSet<string>(defaultVisibleFields);

            PersistFieldsMetadata();
            Notify(entityType);
        }

        #region Data contracts

        [DataContract]
        public class Metadata
        {
            [DataMember]
            public int version;

            [DataMember]
            public Dictionary<string, HashSet<string>> data;
        }

        #endregion

        private readonly Dictionary<string, List<IFieldsObserver>> _fieldsObservers = new Dictionary<string, List<IFieldsObserver>>();

        /// <summary>
        /// Register the given observer
        /// </summary>
        public void Attach(IFieldsObserver observer)
        {
            if (observer == null)
                return;

            List<IFieldsObserver> observers;
            if (!_fieldsObservers.TryGetValue(observer.EntityType, out observers))
            {
                observers = new List<IFieldsObserver>();
                _fieldsObservers[observer.EntityType] = observers;
            }

            // since we are using the same window for displaying
            var existingObserver = observers.FirstOrDefault(o => o.Id == observer.Id);
            if (existingObserver != null)
                observers.Remove(existingObserver);

            observers.Add(observer);
        }

        /// <summary>
        /// Unregister the given observer
        /// </summary>
        public void Detach(IFieldsObserver observer)
        {
            if (observer == null)
                return;

            List<IFieldsObserver> observers;
            if (!_fieldsObservers.TryGetValue(observer.EntityType, out observers))
                return;

            observers.Remove(observer);

            if (observers.Count == 0)
                _fieldsObservers.Remove(observer.EntityType);
        }

        private void Notify(string entityType)
        {
            List<IFieldsObserver> observers;
            if (!_fieldsObservers.TryGetValue(entityType, out observers))
                return;

            foreach (var observer in observers)
            {
                observer.UpdateFields();
            }
        }

        #region FieldsToHide

        /// <summary>
        /// Returns hashset of fields that shouldn't be visible
        /// </summary>
        public static HashSet<string> GetFieldsToHide(BaseEntity entity)
        {
            if (entity == null)
                return new HashSet<string>();

            HashSet<string> result;
            if (!FieldsToHideDictionary.TryGetValue(Utility.GetConcreteEntityType(entity), out result))
            {
                return new HashSet<string>();
            }
            return result;
        }

        private static readonly Dictionary<string, HashSet<string>> FieldsToHideDictionary = new Dictionary<string, HashSet<string>>
        {
            { WorkItem.SUBTYPE_STORY, new HashSet<string>
                        {
                            CommonFields.Name,
                            CommonFields.Phase,
                            BaseEntity.ID_FIELD,
                            CommonFields.HasAttachments,
                            CommonFields.NewTasks,
                            // we want to filter out description because it will be shown separately
                            // and subtype because it is only used internally
                            CommonFields.Description,
                            CommonFields.SubType
                        }
            },
            { WorkItem.SUBTYPE_QUALITY_STORY, new HashSet<string>
                        {
                            CommonFields.Name,
                            CommonFields.Phase,
                            BaseEntity.ID_FIELD,
                            CommonFields.HasAttachments,
                            CommonFields.NewTasks,
                            // we want to filter out description because it will be shown separately
                            // and subtype because it is only used internally
                            CommonFields.Description,
                            CommonFields.SubType
                        }
            },
            { WorkItem.SUBTYPE_DEFECT, new HashSet<string>
                        {
                            CommonFields.Name,
                            CommonFields.Phase,
                            BaseEntity.ID_FIELD,
                            CommonFields.HasAttachments,
                            CommonFields.NewTasks,
                            // we want to filter out description because it will be shown separately
                            // and subtype because it is only used internally
                            CommonFields.Description,
                            CommonFields.SubType
                        }
            },
            { Task.TYPE_TASK, new HashSet<string>
                        {
                            CommonFields.Name,
                            CommonFields.Phase,
                            BaseEntity.ID_FIELD,
                            // we want to filter out description because it will be shown separately
                            // and subtype because it is only used internally
                            CommonFields.Description,
                            CommonFields.SubType
                        }
            },
            { TestGherkin.SUBTYPE_GHERKIN_TEST, new HashSet<string>
                        {
                            CommonFields.CreationTime,
                            CommonFields.Name,
                            CommonFields.Phase,
                            BaseEntity.ID_FIELD,
                            TestGherkin.TEST_STATUS_FIELD,
                            TestGherkin.IDENTITY_HASH_FIELD,
                            CommonFields.HasAttachments,
                            // we want to filter out description because it will be shown separately
                            // and subtype because it is only used internally
                            CommonFields.Description,
                            CommonFields.SubType
                        }
            },
            { Requirement.SUBTYPE_DOCUMENT, new HashSet<string>
                        {
                            CommonFields.Name,
                            CommonFields.Phase,
                            BaseEntity.ID_FIELD,
                            // we want to filter out description because it will be shown separately
                            // and subtype because it is only used internally
                            CommonFields.Description,
                            CommonFields.SubType
                        }
            },
            { Test.SUBTYPE_MANUAL_TEST, new HashSet<string>
                        {
                            CommonFields.CreationTime,
                            CommonFields.Name,
                            CommonFields.Phase,
                            BaseEntity.ID_FIELD,
                            TestGherkin.TEST_STATUS_FIELD,
                            CommonFields.HasAttachments,
                            // we want to filter out description because it will be shown separately
                            // and subtype because it is only used internally
                            CommonFields.Description,
                            CommonFields.SubType
                        }
            },
            { RunManual.SUBTYPE_RUN_MANUAL, new HashSet<string>
                        {
                            CommonFields.Name,
                            BaseEntity.ID_FIELD,
                            CommonFields.HasAttachments,
                            RunManual.HAS_VISUAL_COVERAGE_FIELD,
                            Run.TEST_FIELD,
                            // we want to filter out description because it will be shown separately
                            // and subtype because it is only used internally
                            CommonFields.Description,
                            CommonFields.SubType
                        }
            },
            { RunSuite.SUBTYPE_RUN_SUITE, new HashSet<string>
                        {
                            CommonFields.Name,
                            BaseEntity.ID_FIELD,
                            Run.TEST_FIELD,
                            // we want to filter out description because it will be shown separately
                            // and subtype because it is only used internally
                            CommonFields.Description,
                            CommonFields.SubType
                        }
            }
        };

        #endregion
    }
}
