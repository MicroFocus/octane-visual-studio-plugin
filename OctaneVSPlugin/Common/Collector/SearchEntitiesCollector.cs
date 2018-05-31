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
    /// <summary>
    /// Mechanism for searching the entities
    /// </summary>
    internal class SearchEntitiesCollector : EntitiesCollector
    {
        private readonly string _searchString;
        private readonly int _limitPerType;

        /// <summary>
        /// Constructor
        /// </summary>
        internal SearchEntitiesCollector(OctaneServices service, string searchString, int limitPerType)
            : base(service)
        {
            _searchString = searchString;
            _limitPerType = limitPerType;
        }

        /// <inheritdoc/>>
        protected override void PrepareCollectorTasks()
        {
            RegisterCollectorTask(Service.SearchEntitiesByType<WorkItem>(_searchString, _limitPerType, WorkItem.SUBTYPE_DEFECT));

            RegisterCollectorTask(Service.SearchEntitiesByType<Test>(_searchString, _limitPerType, TestSuite.SUBTYPE_TEST_SUITE));
            RegisterCollectorTask(Service.SearchEntitiesByType<Test>(_searchString, _limitPerType, Test.SUBTYPE_MANUAL_TEST));
            RegisterCollectorTask(Service.SearchEntitiesByType<Test>(_searchString, _limitPerType, TestAutomated.SUBTYPE_TEST_AUTOMATED));
            RegisterCollectorTask(Service.SearchEntitiesByType<Test>(_searchString, _limitPerType, TestGherkin.SUBTYPE_GHERKIN_TEST));

            RegisterCollectorTask(Service.SearchEntitiesByType<WorkItem>(_searchString, _limitPerType, WorkItem.SUBTYPE_STORY));
            RegisterCollectorTask(Service.SearchEntitiesByType<WorkItem>(_searchString, _limitPerType, WorkItem.SUBTYPE_QUALITY_STORY));
            RegisterCollectorTask(Service.SearchEntitiesByType<WorkItem>(_searchString, _limitPerType, WorkItem.SUBTYPE_EPIC));
            RegisterCollectorTask(Service.SearchEntitiesByType<WorkItem>(_searchString, _limitPerType, WorkItem.SUBTYPE_FEATURE));

            RegisterCollectorTask(Service.SearchEntitiesByType<Task>(_searchString, _limitPerType, Task.TYPE_TASK));

            RegisterCollectorTask(Service.SearchEntitiesByType<Requirement>(_searchString, _limitPerType, Requirement.SUBTYPE_DOCUMENT));
        }
    }
}
