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

namespace MicroFocus.Adm.Octane.VisualStudio.Common.Collector
{
    internal class SearchEntitiesCollector : EntitiesCollector
    {
        private readonly string _searchString;
        private readonly int _limitPerType;

        internal SearchEntitiesCollector(OctaneServices service, string searchString, int limitPerType)
            : base(service)
        {
            _searchString = searchString;
            _limitPerType = limitPerType;
        }

        protected override void PrepareCollectorTasks()
        {
            RegisterCollectorTask(Service.SearchEntitiesByType<Defect>(_searchString, _limitPerType));
            RegisterCollectorTask(Service.SearchEntitiesByType<Story>(_searchString, _limitPerType));
        }
    }
}
