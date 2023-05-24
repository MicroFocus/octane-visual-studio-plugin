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
