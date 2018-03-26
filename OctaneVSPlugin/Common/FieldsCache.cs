/*!
* (c) 2016-2018 EntIT Software LLC, a Micro Focus company
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

using MicroFocus.Adm.Octane.VisualStudio.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace MicroFocus.Adm.Octane.VisualStudio.Common
{
    /// <summary>
    /// Cache responsible for maintaining the default fields for each entity type
    /// </summary>
    public class FieldsCache
    {
        private static FieldsCache _fieldsCache;
        private static readonly object _obj = new object();

        private static readonly DataContractJsonSerializer _serializer = new DataContractJsonSerializer(typeof(Metadata));

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

        private static string Serialize()
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                _serializer.WriteObject(memoryStream, _persistedFieldsCache);

                memoryStream.Position = 0;
                using (StreamReader sr = new StreamReader(memoryStream))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        private static Metadata Deserialize(string json)
        {
            try
            {
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                {
                    return (Metadata)_serializer.ReadObject(stream);
                }
            }
            catch (Exception)
            {
                return new Metadata
                {
                    data = new Dictionary<string, HashSet<string>>(),
                    version = 1
                };
            }
        }

        private static void DeserializeDefaultFieldsMetadata()
        {
            var assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream("MicroFocus.Adm.Octane.VisualStudio.Resources.DefaultFields.json"))
            using (StreamReader reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();
                _defaultFieldsCache = Deserialize(result);
            }
        }

        private static void DeserializePersistedFieldsMetadata()
        {
            var persistedJson = OctanePluginSettings.Default.EntityFields;
            _persistedFieldsCache = Deserialize(persistedJson);
        }

        private static void PersistFieldsMetadata()
        {
            try
            {
                OctanePluginSettings.Default.EntityFields = Serialize();
                OctanePluginSettings.Default.Save();
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Check whether the input fields are the same as the default fields for the given entity type
        /// </summary>
        public bool AreSameFieldsAsDefaultFields(string entityType, List<FieldViewModel> visibleFields)
        {
            if (string.IsNullOrEmpty(entityType))
                return false;

            HashSet<string> defaultVisibleFields;
            if (!_defaultFieldsCache.data.TryGetValue(entityType, out defaultVisibleFields))
                return false;

            if (visibleFields.Count != defaultVisibleFields.Count)
                return false;

            return visibleFields.All(f => defaultVisibleFields.Contains(f.Name));
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
        }

        /// <summary>
        /// Reset to the default visible fields for the given entity
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
        }

        #region Data contracts

        [DataContract]
        private class Metadata
        {
            [DataMember]
            public int version;

            [DataMember]
            public Dictionary<string, HashSet<string>> data;
        }

        #endregion
    }
}
