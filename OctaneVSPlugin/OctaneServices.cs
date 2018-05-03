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

using MicroFocus.Adm.Octane.Api.Core.Connector;
using MicroFocus.Adm.Octane.Api.Core.Entities;
using MicroFocus.Adm.Octane.Api.Core.Services;
using MicroFocus.Adm.Octane.Api.Core.Services.Query;
using MicroFocus.Adm.Octane.Api.Core.Services.RequestContext;
using MicroFocus.Adm.Octane.VisualStudio.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OctaneTask = MicroFocus.Adm.Octane.Api.Core.Entities.Task;
using Task = System.Threading.Tasks.Task;

namespace MicroFocus.Adm.Octane.VisualStudio
{
    /// <summary>
    /// Responsible for interacting with Octane API.
    /// </summary>
    internal class OctaneServices
    {
        private RestConnector rest;
        private EntityService es;

        private string url;
        private string user;
        private string password;
        private WorkspaceContext workspaceContext;
        private SharedSpaceContext sharedSpaceContext;

        private static readonly EntityComparerByLastModified entityComparer = new EntityComparerByLastModified();

        public OctaneServices(string url, long sharedspaceId, long workspaceId, string user, string password)
        {
            this.url = url;

            this.user = user;
            this.password = password;

            rest = new RestConnector();
            es = new EntityService(rest);

            workspaceContext = new WorkspaceContext(sharedspaceId, workspaceId);
            sharedSpaceContext = new SharedSpaceContext(sharedspaceId);
        }

        public async Task Connect()
        {
            if (!rest.IsConnected())
            {
                await rest.ConnectAsync(url, new UserPassConnectionInfo(user, password));
            }

        }

        private IList<QueryPhrase> ToQueryList(QueryPhrase query)
        {
            List<QueryPhrase> queries = new List<QueryPhrase>
            {
                query
            };
            return queries;
        }

        private IList<QueryPhrase> BuildUserItemCriteria(WorkspaceUser user)
        {
            List<QueryPhrase> queries = new List<QueryPhrase>
            {
                new CrossQueryPhrase("user", new LogicalQueryPhrase("id", user.Id)),
            };

            return queries;
        }

        private LogicalQueryPhrase BuildCriteria(List<UserItem> userItems, Func<UserItem, BaseEntity> getReferenceEntityFunc)
        {
            var idPhrase = new LogicalQueryPhrase("id");

            foreach (UserItem userItem in userItems)
            {
                BaseEntity referenceEntity = getReferenceEntityFunc(userItem);
                if (referenceEntity != null)
                {
                    idPhrase.AddExpression(referenceEntity.Id, ComparisonOperator.Equal);
                }
            }

            return idPhrase;
        }

        public async Task<IList<BaseEntity>> GetMyItems(MyWorkMetadata itemFetchInfo)
        {
            var owner = await GetWorkspaceUser();
            EntityListResult<UserItem> userItems = await es.GetAsync<UserItem>(workspaceContext,
                BuildUserItemCriteria(owner), BuildUserItemFields());

            var collector = new EntitiesCollector(this, userItems, itemFetchInfo);

            collector.Add<WorkItem>(userItem => userItem.WorkItem);
            collector.Add<Test>(userItem => userItem.Test);
            collector.Add<Run>(userItem => userItem.Run);
            collector.Add<Requirement>(userItem => userItem.Requirement);
            collector.Add<OctaneTask>(userItem => userItem.Task);

            List<BaseEntity> result = await collector.GetAllEntities();
            result.Sort(entityComparer);

            return result;
        }

        private IList<QueryPhrase> BuildCommentsCriteria(WorkspaceUser user)
        {
            return new List<QueryPhrase>
            {
                new CrossQueryPhrase("mention_user", new LogicalQueryPhrase("id", user.Id))
            };
        }

        private readonly List<string> commentFields = new List<string>
        {
            Comment.AUTHOR_FIELD,
            Comment.OWNER_WORK_FIELD,
            Comment.OWNER_TEST_FIELD,
            Comment.OWNER_RUN_FIELD,
            Comment.OWNER_REQUIREMENT_FIELD,
            Comment.CREATION_TIME_FIELD,
            Comment.TEXT_FIELD
        };

        public async Task<IList<BaseEntity>> GetMyCommentItems()
        {
            var owner = await GetWorkspaceUser();
            EntityListResult<Comment> comments = await es.GetAsync<Comment>(workspaceContext, BuildCommentsCriteria(owner), commentFields);
            return comments.BaseEntities.ToList();
        }

        private async Task<WorkspaceUser> GetWorkspaceUser()
        {
            QueryPhrase ownerQuery = new LogicalQueryPhrase("name", user);
            EntityListResult<WorkspaceUser> ownerQueryResult = await es.GetAsync<WorkspaceUser>(workspaceContext, ToQueryList(ownerQuery), null);
            var workspaceUser = ownerQueryResult.data.FirstOrDefault();
            if (workspaceUser == null)
                throw new Exception($"Unable to find a user with the name \"{user}\"");
            return workspaceUser;
        }

        public async Task<BaseEntity> FindEntity(BaseEntity entityModel, IList<string> fields)
        {
            var entity = await es.GetByIdAsync(workspaceContext, entityModel.Id, Utility.GetBaseEntityType(entityModel), fields);
            return entity;
        }

        private readonly Dictionary<string, string> commentSupport = new Dictionary<string, string>
        {
            { "work_item", "owner_work_item" },
            { "test", "owner_test" },
            { "run", "owner_run" },
            { "requirement", "owner_requirement" }
        };

        /// <summary>
        /// Retrieves a list of all the comments attached to the given entity
        /// </summary>
        public async Task<List<Comment>> GetAttachedCommentsToEntity(BaseEntity entity)
        {
            if (string.IsNullOrEmpty(entity.AggregateType))
                return new List<Comment>();

            if (!commentSupport.TryGetValue(entity.AggregateType, out var type))
                return new List<Comment>();

            var query = new List<QueryPhrase>
            {
                new CrossQueryPhrase(type, new LogicalQueryPhrase("id", entity.Id))
            };
            var comments = await es.GetAsync<Comment>(workspaceContext, query, commentFields);
            return comments?.data;
        }

        private Task<EntityListResult<TEntity>> FetchEntities<TEntity>(
            List<UserItem> userItems,
            Func<UserItem, BaseEntity> getReferenceEntityFunc,
            MyWorkMetadata itemFetchInfo)
            where TEntity : BaseEntity
        {
            LogicalQueryPhrase idCriteria = BuildCriteria(userItems, getReferenceEntityFunc);
            if (idCriteria.Expressions.Count == 0)
            {
                // There are no items to select, return empty list.
                return Task.FromResult(new EntityListResult<TEntity>());
            }

            Task<EntityListResult<TEntity>> fetchEntitiesTask = es.GetAsync<TEntity>(workspaceContext,
                ToQueryList(idCriteria),
                itemFetchInfo.FieldsForType<TEntity>());

            return fetchEntitiesTask;
        }

        public Task<TestScript> GetTestScript(EntityId id)
        {
            return es.GetTestScriptAsync(workspaceContext, id);
        }

        private List<string> BuildUserItemFields()
        {
            string[] fields = new[]
            {
                UserItem.ENTITY_TYPE_FIELD,
                UserItem.REASON_FIELD,
                UserItem.USER_FIELD,
                UserItem.WORK_ITEM_REFERENCE,
                UserItem.TEST_REFERENCE,
                UserItem.RUN_REFERENCE,
                UserItem.REQUIREMENT_REFERENCE,
                UserItem.TASK_REFERENCE
            };
            return fields.ToList();
        }

        /// <summary>
        /// Return the fields' metadata for the given entity type
        /// </summary>
        public async Task<List<FieldMetadata>> GetFieldsMetadata(string entityType)
        {
            var result = await es.GetFieldsMetadataAsync(workspaceContext, entityType);
            return result?.data;
        }

        private class EntitiesCollector
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
}
