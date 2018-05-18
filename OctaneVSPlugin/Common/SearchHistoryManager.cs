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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace MicroFocus.Adm.Octane.VisualStudio.Common
{
    public static class SearchHistoryManager
    {
        private const int MaxSearchHistorySize = 4;
        private static SearchHistoryMetadata _metadata;

        internal static List<string> LoadHistory()
        {
            _metadata = Utility.DeserializeFromJson(OctanePluginSettings.Default.SearchHistory, new SearchHistoryMetadata
            {
                id = ConstructId(),
                queries = new List<string>()
            });

            return _metadata.queries.ToList();
        }

        private static void HandleDifferentContext()
        {
            if (_metadata.id == ConstructId())
            {
                return;
            }

            _metadata = new SearchHistoryMetadata
            {
                id = ConstructId(),
                queries = new List<string>()
            };

            SaveHistory();
        }

        internal static void UpdateHistory(string filter)
        {
            var newHistory = _metadata.queries.ToList();
            var oldHistory = _metadata.queries.ToList();

            newHistory.Clear();
            newHistory.Add(filter);

            oldHistory.Remove(filter);

            newHistory.AddRange(oldHistory.Take(MaxSearchHistorySize));

            _metadata.queries = newHistory;

            SaveHistory();
        }

        internal static List<string> GetHistory()
        {
            HandleDifferentContext();
            return _metadata.queries.ToList();
        }

        private static void SaveHistory()
        {
            try
            {
                OctanePluginSettings.Default.SearchHistory = Utility.SerializeToJson(_metadata);
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
        public class SearchHistoryMetadata
        {
            [DataMember]
            public string id;

            [DataMember]
            public List<string> queries;
        }
    }
}
