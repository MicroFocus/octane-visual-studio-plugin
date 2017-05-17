using Hpe.Nga.Api.Core.Connector;
using Hpe.Nga.Api.Core.Entities;
using Hpe.Nga.Api.Core.Services;
using Hpe.Nga.Api.Core.Services.Query;
using Hpe.Nga.Api.Core.Services.RequestContext;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hpe.Nga.Octane.VisualStudio
{
    class OctaneServices
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
        public bool Connect()
        {
            if (rest.IsConnected())
            {
                return true;
            }
            
            return rest.Connect(url, user, password);

        }
        private IList<QueryPhrase> ToQueryList(QueryPhrase query)
        {
            List<QueryPhrase> queries = new List<QueryPhrase>();
            queries.Add(query);
            return queries;
        }
        public IList<WorkItem> GetMyItems()
        {

            // get the id of the logged in user
            QueryPhrase ownerQuery = new LogicalQueryPhrase("email", this.user);
            var owner = es.Get<SharedspaceUser>(sharedSpaceContext, ToQueryList(ownerQuery), null).data.FirstOrDefault();

            // get the items owned by the user
            QueryPhrase ownerItemsQuery = new CrossQueryPhrase("owner", new LogicalQueryPhrase("id", owner.Id));
            var results = es.Get<WorkItem>(workspaceContext,ToQueryList(ownerItemsQuery),null);
            return results.data;
        } 
    }
}
