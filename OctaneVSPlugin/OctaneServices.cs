using Hpe.Nga.Api.Core.Connector;
using Hpe.Nga.Api.Core.Entities;
using Hpe.Nga.Api.Core.Services;
using Hpe.Nga.Api.Core.Services.Query;
using Hpe.Nga.Api.Core.Services.RequestContext;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

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

            Task<EntityListResult<WorkItem>> workItemsTask = FetchEntities<WorkItem>(
                userItems.data,
                userItem => userItem.WorkItem, 
                itemFetchInfo);

            Task<EntityListResult<Test>> testTask = FetchEntities<Test>(
                userItems.data,
                userItem => userItem.Test,
                itemFetchInfo);

            Task<EntityListResult<Run>> testRunTask = FetchEntities<Run>(
                userItems.data,
                userItem => userItem.Run,
                itemFetchInfo);

            Task<EntityListResult<Requirement>> requirementTask = FetchEntities<Requirement>(
                userItems.data,
                userItem => userItem.Requirement,
                itemFetchInfo);

            // Filter only the subtypes requested.
            List<WorkItem> workItems = (await workItemsTask).data;
            List<Test> tests = (await testTask).data;
            List<Run> testRuns = (await testRunTask).data;
            List<Requirement> requirements = (await requirementTask).data;

            List<BaseEntity> result = new List<BaseEntity>();
            result.AddRange(workItems);
            result.AddRange(tests);
            result.AddRange(testRuns);
            result.AddRange(requirements);

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
                UserItem.REQUIREMENT_REFERENCE
            };
            return fields.ToList();
        }
    }
}
