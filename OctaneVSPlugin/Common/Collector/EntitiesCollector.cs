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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace MicroFocus.Adm.Octane.VisualStudio.Common.Collector
{
    internal class EntitiesCollector
    {
        private readonly OctaneServices octaneService;
        private readonly EntityListResult<UserItem> userItems;
        private readonly MyWorkMetadata itemFetchInfo;

        private List<Task<GenericEntityListResult>> fetchTasks;

        public EntitiesCollector(OctaneServices octaneService, EntityListResult<UserItem> userItems, MyWorkMetadata itemFetchInfo)
        {
            this.octaneService = octaneService;
            this.userItems = userItems;
            this.itemFetchInfo = itemFetchInfo;

            fetchTasks = new List<Task<GenericEntityListResult>>();
        }

        public async Task<List<BaseEntity>> GetAllEntities()
        {
            await Task.WhenAll(fetchTasks.ToArray());

            var allEntities =
                from task in fetchTasks
                let result = task.Result.BaseEntities
                select result;

            // Flat the result list and materialize it with ToList.
            return allEntities.SelectMany(x => x).ToList();
        }

        public void Add<TEntityType>(Func<UserItem, BaseEntity> getReferenceEntityFunc) where TEntityType : BaseEntity
        {
            var tcs = new TaskCompletionSource<GenericEntityListResult>();

            Task<EntityListResult<TEntityType>> fetchTask = octaneService.FetchEntities<TEntityType>(
                userItems.data,
                getReferenceEntityFunc,
                itemFetchInfo);

            // This trick turns a Task<EntityListResult<TEntity>> to generic Task<GenericEntityListResult>.
            // This allows us to later aggregate all the fetched entities without caring about the specific type of each one.
            fetchTask.ContinueWith((result) =>
            {
                tcs.TrySetResult(fetchTask.Result);
            });

            fetchTasks.Add(tcs.Task);
        }
    }
}
