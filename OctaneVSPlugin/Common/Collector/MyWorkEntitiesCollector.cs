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
using MicroFocus.Adm.Octane.Api.Core.Services;
using System;
using System.Threading.Tasks;
using OctaneTask = MicroFocus.Adm.Octane.Api.Core.Entities.Task;

namespace MicroFocus.Adm.Octane.VisualStudio.Common.Collector
{
    /// <summary>
    /// Mechanism for obtaining the entities under 'My Work'
    /// </summary>
    internal class MyWorkEntitiesCollector : EntitiesCollector
    {
        private readonly EntityListResult<UserItem> _userItems;
        private readonly MyWorkMetadata _itemFetchInfo;

        /// <summary>
        /// Constructor
        /// </summary>
        internal MyWorkEntitiesCollector(OctaneServices service, EntityListResult<UserItem> userItems, MyWorkMetadata itemFetchInfo)
            : base(service)
        {
            _userItems = userItems;
            _itemFetchInfo = itemFetchInfo;
        }

        /// <inheritdoc/>>
        protected override void PrepareCollectorTasks()
        {
            Add<WorkItem>(userItem => userItem.WorkItem);
            Add<Test>(userItem => userItem.Test);
            Add<Run>(userItem => userItem.Run);
            Add<Requirement>(userItem => userItem.Requirement);
            Add<OctaneTask>(userItem => userItem.Task);
        }

        private void Add<TEntityType>(Func<UserItem, BaseEntity> getReferenceEntityFunc) where TEntityType : BaseEntity
        {
            Task<EntityListResult<TEntityType>> fetchTask = Service.FetchEntities<TEntityType>(
                _userItems.data,
                getReferenceEntityFunc,
                _itemFetchInfo);

            RegisterCollectorTask(fetchTask);
        }
    }
}
