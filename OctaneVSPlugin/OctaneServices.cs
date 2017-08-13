using Hpe.Nga.Api.Core.Connector;
using Hpe.Nga.Api.Core.Entities;
using Hpe.Nga.Api.Core.Services;
using Hpe.Nga.Api.Core.Services.Query;
using Hpe.Nga.Api.Core.Services.RequestContext;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

using Task = System.Threading.Tasks.Task;
using OctaneTask = Hpe.Nga.Api.Core.Entities.Task; 

namespace Hpe.Nga.Octane.VisualStudio
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
            // get the id of the logged in user
            QueryPhrase ownerQuery = new LogicalQueryPhrase("email", this.user);
            EntityListResult<WorkspaceUser> ownerQueryResult = await es.GetAsync<WorkspaceUser>(workspaceContext, ToQueryList(ownerQuery), null);
            WorkspaceUser owner = ownerQueryResult.data.FirstOrDefault();

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
