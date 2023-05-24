﻿/*!
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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace MicroFocus.Adm.Octane.VisualStudio.Common.Collector
{
    /// <summary>
    /// Generic behaviour for collecting entities of different types
    /// The collecting mechanism must be implemented by derived classes
    /// </summary>
    internal abstract class EntitiesCollector
    {
        protected readonly OctaneServices Service;

        private readonly List<Task<GenericEntityListResult>> _fetchTasks;

        protected EntitiesCollector(OctaneServices service)
        {
            Service = service;

            _fetchTasks = new List<Task<GenericEntityListResult>>();
        }

        protected abstract void PrepareCollectorTasks();

        /// <summary>
        /// Return a flat list with all the entities
        /// </summary>
        public async Task<List<BaseEntity>> GetAllEntities()
        {
            PrepareCollectorTasks();

            await Task.WhenAll(_fetchTasks.ToArray());
            // Flat the result list and materialize it with ToList.
            return _fetchTasks.SelectMany(t => t.Result.BaseEntities).ToList();
        }

        protected void RegisterCollectorTask<TEntityType>(Task<EntityListResult<TEntityType>> collectorTask) where TEntityType : BaseEntity
        {
            var tcs = new TaskCompletionSource<GenericEntityListResult>();

            // This trick turns a Task<EntityListResult<TEntity>> to generic Task<GenericEntityListResult>.
            // This allows us to later aggregate all the fetched entities without caring about the specific type of each one.
            collectorTask.ContinueWith(result =>
            {
                if (result.IsFaulted || result.IsCanceled)
                {
                    // return an empty list in case something went wrong with the collector task
                    tcs.TrySetResult(new EntityListResult<TEntityType>());
                }
                else
                {
                    tcs.TrySetResult(collectorTask.Result);
                }
            });

            _fetchTasks.Add(tcs.Task);
        }
    }
}
