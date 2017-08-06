using Hpe.Nga.Api.Core.Connector;
using Hpe.Nga.Api.Core.Entities;
using Hpe.Nga.Api.Core.Services;
using Hpe.Nga.Api.Core.Services.Query;
using Hpe.Nga.Api.Core.Services.RequestContext;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<IList<WorkItem>> GetMyItems(ISet<string> subtypes)
        {
            // get the id of the logged in user
            QueryPhrase ownerQuery = new LogicalQueryPhrase("email", this.user);
            EntityListResult<WorkspaceUser> ownerQueryResult = await es.GetAsync<WorkspaceUser>(workspaceContext, ToQueryList(ownerQuery), null);
            WorkspaceUser owner = ownerQueryResult.data.FirstOrDefault();

            // get the items owned by the user
            QueryPhrase ownerItemsQuery = new CrossQueryPhrase("owner", new LogicalQueryPhrase("id", owner.Id));
            EntityListResult<WorkItem> results = await es.GetAsync<WorkItem>(workspaceContext,ToQueryList(ownerItemsQuery),null);

            // Filter only the subtypes requested.
            return results.data.Where(item => subtypes.Contains(item.SubType)).ToList();
        } 
    }
}
