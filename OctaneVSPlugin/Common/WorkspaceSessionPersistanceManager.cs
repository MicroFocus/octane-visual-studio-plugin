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

using MicroFocus.Adm.Octane.Api.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace MicroFocus.Adm.Octane.VisualStudio.Common
{
    /// <summary>
    /// Class for managing current workspace session information
    /// </summary>
    public static class WorkspaceSessionPersistanceManager
    {
        private static WorkspaceSessionMetadata _metadata;

        #region History

        /// <summary>
        /// Maximum number of elements saved for the search history
        /// </summary>
        internal const int MaxSearchHistorySize = 5;

        /// <summary>
        /// Update the search history with the given filter
        /// </summary>
        internal static void UpdateHistory(string filter)
        {
            if (string.IsNullOrEmpty(filter))
                return;

            LoadMetadataIfNeeded();

            var newHistory = _metadata.queries.ToList();
            var oldHistory = _metadata.queries.ToList();

            newHistory.Clear();
            newHistory.Add(filter);

            oldHistory.Remove(filter);

            newHistory.AddRange(oldHistory.Take(MaxSearchHistorySize - 1));

            _metadata.queries = newHistory;

            SaveMetadata();
        }

        /// <summary>
        /// Returns the current search history
        /// </summary>
        internal static List<string> History
        {
            get
            {
                LoadMetadataIfNeeded();
                if (_metadata.queries.Count != 0)
                {
                    return _metadata.queries.ToList();
                }
                else
                {
                    return new List<string> { "" };
                }
            }
        }

        #endregion

        #region ActiveEntity

        /// <summary>
        /// Check whether the given entity is the currently cached active entity
        /// </summary>
        internal static bool IsActiveEntity(BaseEntity entity)
        {
            if (entity == null)
                return false;

            LoadMetadataIfNeeded();

            return _metadata.activeEntityId == entity.Id
                    && _metadata.activeEntityType == Utility.GetConcreteEntityType(entity);
        }

        /// <summary>
        /// Returns the currently active entity
        /// </summary>
        internal static BaseEntity GetActiveEntity()
        {
            LoadMetadataIfNeeded();

            if (string.IsNullOrEmpty(_metadata.activeEntityType))
                return null;

            var entity = new BaseEntity(_metadata.activeEntityId);
            entity.SetValue(BaseEntity.TYPE_FIELD, _metadata.activeEntityType);
            return entity;
        }

        /// <summary>
        /// Sets the given entity as the currently active entity
        /// </summary>
        internal static void SetActiveEntity(BaseEntity entity)
        {
            if (entity == null)
                return;

            LoadMetadataIfNeeded();

            _metadata.activeEntityType = Utility.GetConcreteEntityType(entity);
            _metadata.activeEntityId = entity.Id;

            SaveMetadata();
        }

        /// <summary>
        /// Clear the cache of the currently active entity
        /// </summary>
        internal static void ClearActiveEntity()
        {
            LoadMetadataIfNeeded();

            _metadata.activeEntityType = string.Empty;
            _metadata.activeEntityId = string.Empty;

            SaveMetadata();
        }

        #endregion

        private static void LoadMetadataIfNeeded()
        {
            DeserializeMetadataIfNeeded();
            HandleDifferentContext();
        }

        private static void DeserializeMetadataIfNeeded()
        {
            if (_metadata != null)
                return;

            _metadata = Utility.DeserializeFromJson(OctanePluginSettings.Default.WorkspaceSessionMetadata, new WorkspaceSessionMetadata
            {
                id = ConstructId(),
                queries = new List<string>(),
                activeEntityType = string.Empty,
                activeEntityId = string.Empty
            });
        }

        private static void HandleDifferentContext()
        {
            if (_metadata.id == ConstructId())
            {
                return;
            }

            _metadata = new WorkspaceSessionMetadata
            {
                id = ConstructId(),
                queries = new List<string>(),
                activeEntityType = string.Empty,
                activeEntityId = string.Empty
            };

            SaveMetadata();
        }

        private static void SaveMetadata()
        {
            try
            {
                OctanePluginSettings.Default.WorkspaceSessionMetadata = Utility.SerializeToJson(_metadata);
                OctanePluginSettings.Default.Save();
            }
            catch (Exception)
            {
            }
        }

        private static string ConstructId()
        {
            return OctaneConfiguration.Url + OctaneConfiguration.SharedSpaceId + OctaneConfiguration.WorkSpaceId + OctaneConfiguration.Username;
        }

        [DataContract]
        public class WorkspaceSessionMetadata
        {
            [DataMember]
            public string id;

            [DataMember]
            public List<string> queries;

            [DataMember]
            public string activeEntityType;

            [DataMember]
            public string activeEntityId;
        }
    }
}
