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

        private IList<QueryPhrase> BuildWorkItemCriteria<TEntityType>(WorkspaceUser user, MyWorkMetadata itemFetchInfo, string[] phasePartialLogicalNames)
        {
            var phaseCriteria = new LogicalQueryPhrase("logical_name");
            ISet<string> subTypes = itemFetchInfo.SubtypesForType<TEntityType>();
            AddSubTypesPhaseCriteria(phaseCriteria, subTypes, phasePartialLogicalNames);

            var subTypeCriteria = new LogicalQueryPhrase("subtype");
            foreach (string subType in subTypes)
            {
                subTypeCriteria.AddExpression(subType, ComparisonOperator.Equal);
            }

            List<QueryPhrase> queries = new List<QueryPhrase>
            {
                new CrossQueryPhrase("owner", new LogicalQueryPhrase("id", user.Id)),
                new CrossQueryPhrase("phase", phaseCriteria)
            };

            return queries;
        }

        private IList<QueryPhrase> BuildTestRunCriteria(WorkspaceUser user, MyWorkMetadata itemFetchInfo)
        {
            string[] phasePartialLogicalNames = new string[] {
                "list_node.run_native_status.blocked",
                "list_node.run_native_status.not_completed",
                "list_node.run_native_status.planned" };

            var nativeStatusCriteria = new LogicalQueryPhrase("logical_name");
            foreach (string statusLogicalName in phasePartialLogicalNames)
            {
                nativeStatusCriteria.AddExpression(statusLogicalName, ComparisonOperator.Equal);
            }

            var subTypeCriteria = new LogicalQueryPhrase("subtype");
            foreach (string subType in itemFetchInfo.SubtypesForType<Run>())
            {
                subTypeCriteria.AddExpression(subType, ComparisonOperator.Equal);
            }

            List<QueryPhrase> queries = new List<QueryPhrase>
            {
                new CrossQueryPhrase("run_by", new LogicalQueryPhrase("id", user.Id)),
                new CrossQueryPhrase("native_status", nativeStatusCriteria),
                new CrossQueryPhrase("parent_suite", NullQueryPhrase.Null)
            };

            return queries;
        }

        private IList<QueryPhrase> BuildRequirementCriteria(WorkspaceUser user, MyWorkMetadata itemFetchInfo)
        {
            string[] phasePartialLogicalNames = new string[] {
                "phase.requirement_document.draft",
                "phase.requirement_document.accepted",
                "phase.requirement_document.indesign" };

            var phaseCriteria = new LogicalQueryPhrase("logical_name");
            foreach (string statusLogicalName in phasePartialLogicalNames)
            {
                phaseCriteria.AddExpression(statusLogicalName, ComparisonOperator.Equal);
            }

            var subTypeCriteria = new LogicalQueryPhrase("subtype");
            foreach (string subType in itemFetchInfo.SubtypesForType<Requirement>())
            {
                subTypeCriteria.AddExpression(subType, ComparisonOperator.Equal);
            }

            List<QueryPhrase> queries = new List<QueryPhrase>
            {
                new CrossQueryPhrase("owner", new LogicalQueryPhrase("id", user.Id)),
                new CrossQueryPhrase("phase", phaseCriteria)
            };

            return queries;
        }

        private void AddSubTypesPhaseCriteria(LogicalQueryPhrase phaseCriteria, ISet<string> workItemSubTypes, string[] phasePartialLogicalNames)
        {
            foreach (string subType in workItemSubTypes)
            {
                foreach (string phasePartialLogicalName in phasePartialLogicalNames)
                {
                    phaseCriteria.Expressions.Add(new QueryExpression(string.Format("phase.{0}.{1}", subType, phasePartialLogicalName)));
                }
            }
        }

        public async Task<IList<BaseEntity>> GetMyItems(MyWorkMetadata itemFetchInfo)
        {
            // get the id of the logged in user
            QueryPhrase ownerQuery = new LogicalQueryPhrase("email", this.user);
            EntityListResult<WorkspaceUser> ownerQueryResult = await es.GetAsync<WorkspaceUser>(workspaceContext, ToQueryList(ownerQuery), null);
            WorkspaceUser owner = ownerQueryResult.data.FirstOrDefault();

            Task<EntityListResult<WorkItem>> workItemsTask = es.GetAsync<WorkItem>(workspaceContext, 
                BuildWorkItemCriteria<WorkItem>(owner, itemFetchInfo, new string[] { "new", "inprogress", "intesting" }), 
                itemFetchInfo.FieldsForType<WorkItem>());

            Task<EntityListResult<Test>> testTask = es.GetAsync<Test>(workspaceContext,
                BuildWorkItemCriteria<Test>(owner, itemFetchInfo, new string[] { "new", "indesign" }),
                itemFetchInfo.FieldsForType<Test>());

            Task<EntityListResult<Run>> testRunTask = es.GetAsync<Run>(workspaceContext, BuildTestRunCriteria(owner, itemFetchInfo), itemFetchInfo.FieldsForType<Run>());

            Task<EntityListResult<Requirement>> requirementTask = es.GetAsync<Requirement>(workspaceContext, 
                BuildRequirementCriteria(owner, itemFetchInfo), 
                itemFetchInfo.FieldsForType<Requirement>());

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
    }
}
