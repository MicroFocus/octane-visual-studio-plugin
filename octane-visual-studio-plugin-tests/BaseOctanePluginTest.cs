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
using MicroFocus.Adm.Octane.VisualStudio.Tests.Utilities;
using MicroFocus.Adm.Octane.VisualStudio.Tests.Utilities.Entity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace MicroFocus.Adm.Octane.VisualStudio.Tests
{
    /// <summary>
    /// Base test class for plugin tests
    /// </summary>
    [TestClass]
    public abstract class BaseOctanePluginTest
    {
        private dynamic _persistedFieldsCache;

        protected static RestConnector RestConnector = new RestConnector();
        public static EntityService EntityService = new EntityService(RestConnector);

        public static WorkspaceContext WorkspaceContext;

        public static WorkspaceUser User;

        public static Release CurrentRelease;

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
			// Check if .runsettings is configured properly
			EnsurePropertiesSet(context.Properties, 
				"ignoreServerCertificateValidation", 
				"webAppUrl",
				"userName",
				"password",
				"sharedSpaceId",
				"workspaceId");

			var ignoreServerCertificateValidation = context.Properties["ignoreServerCertificateValidation"].ToString();
            if (ignoreServerCertificateValidation != null && ignoreServerCertificateValidation.ToLower().Equals("true"))
            {
                NetworkSettings.IgnoreServerCertificateValidation();
            }
            NetworkSettings.EnableAllSecurityProtocols();
		
			OctaneConfiguration.Url = context.Properties["webAppUrl"].ToString();
            OctaneConfiguration.Username = context.Properties["userName"].ToString();
            OctaneConfiguration.Password = context.Properties["password"].ToString();
            
            var connectionInfo = new UserPassConnectionInfo(OctaneConfiguration.Username, OctaneConfiguration.Password);

            RestConnector.Connect(OctaneConfiguration.Url, connectionInfo);

            var sharedSpaceId = int.Parse(context.Properties["sharedSpaceId"].ToString());
            var workspaceId = int.Parse(context.Properties["workspaceId"].ToString());

            WorkspaceContext = new WorkspaceContext(sharedSpaceId, workspaceId);
            OctaneConfiguration.WorkSpaceId = WorkspaceContext.WorkspaceId;

            var sharedSpaceContext = new SharedSpaceContext(sharedSpaceId);
            OctaneConfiguration.SharedSpaceId = sharedSpaceContext.SharedSpaceId;

            OctaneConfiguration.SsoLogin = bool.Parse(context.Properties["ssoLogin"].ToString());
            OctaneConfiguration.CredentialLogin = bool.Parse(context.Properties["credentialLogin"].ToString());

            User = GetWorkspaceUser();

            CurrentRelease = ReleaseUtilities.CreateRelease();
        }

		private static void EnsurePropertiesSet(IDictionary dictionary, params string[] properties)
		{
			foreach (string prop in properties)
			{
				if (!dictionary.Contains(prop))
				{
					throw new Exception("Test context missing property \"" + prop + "\". Is the .runsettings file configured? Check default.runsettings file.");
				}
			}
		}		

		[AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            EntityService.DeleteById<Release>(WorkspaceContext, CurrentRelease.Id);
        }

        [TestInitialize]
        public void TestInitialize()
        {
            var instance = FieldsCache.Instance;

            var dynamicFieldsCache = ExposedClass.From(typeof(FieldsCache));
            var cache = dynamicFieldsCache._persistedFieldsCache as FieldsCache.Metadata;
            _persistedFieldsCache = Utilities.Utility.Clone(cache);
            dynamicFieldsCache._persistedFieldsCache = new FieldsCache.Metadata
            {
                data = new Dictionary<string, HashSet<string>>(),
                version = 1
            };

            TestInitializeInternal();
        }

        protected virtual void TestInitializeInternal()
        {
        }

        [TestCleanup]
        public void TestCleanup()
        {
            ExposedClass.From(typeof(FieldsCache))._persistedFieldsCache = _persistedFieldsCache;
            ExposedClass.From(typeof(FieldsCache)).PersistFieldsMetadata();

            TestCleanupInternal();
        }

        protected virtual void TestCleanupInternal()
        {
        }

        private static WorkspaceUser GetWorkspaceUser()
        {
            QueryPhrase ownerQuery = new LogicalQueryPhrase("name", OctaneConfiguration.Username);
            EntityListResult<WorkspaceUser> ownerQueryResult = EntityService.GetAsync<WorkspaceUser>(WorkspaceContext, new List<QueryPhrase> { ownerQuery }, null).Result;
            var workspaceUser = ownerQueryResult.data.FirstOrDefault();
            if (workspaceUser == null)
                throw new Exception($"Unable to find a user with the name \"{OctaneConfiguration.Username}\"");
            return workspaceUser;
        }
    }
}
