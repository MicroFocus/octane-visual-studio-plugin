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
using MicroFocus.Adm.Octane.Api.Core.Services;
using MicroFocus.Adm.Octane.Api.Core.Services.RequestContext;
using MicroFocus.Adm.Octane.VisualStudio.Common;
using MicroFocus.Adm.Octane.VisualStudio.Tests.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Configuration;

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
        protected static EntityService EntityService = new EntityService(RestConnector);

        protected static WorkspaceContext WorkspaceContext;

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            var ignoreServerCertificateValidation = ConfigurationManager.AppSettings["ignoreServerCertificateValidation"];
            if (ignoreServerCertificateValidation != null && ignoreServerCertificateValidation.ToLower().Equals("true"))
            {
                NetworkSettings.IgnoreServerCertificateValidation();
            }
            NetworkSettings.EnableAllSecurityProtocols();

            OctaneConfiguration.Url = ConfigurationManager.AppSettings["webAppUrl"];
            // If webAppUrl is empty we do not try to connect.
            if (string.IsNullOrWhiteSpace(OctaneConfiguration.Url))
                return;

            OctaneConfiguration.Username = ConfigurationManager.AppSettings["userName"];
            OctaneConfiguration.Password = ConfigurationManager.AppSettings["password"];
            var connectionInfo = new UserPassConnectionInfo(OctaneConfiguration.Username, OctaneConfiguration.Password);

            RestConnector.Connect(OctaneConfiguration.Url, connectionInfo);


            var sharedSpaceId = int.Parse(ConfigurationManager.AppSettings["sharedSpaceId"]);
            var workspaceId = int.Parse(ConfigurationManager.AppSettings["workspaceId"]);

            WorkspaceContext = new WorkspaceContext(sharedSpaceId, workspaceId);
            OctaneConfiguration.WorkSpaceId = WorkspaceContext.WorkspaceId;

            var sharedSpaceContext = new SharedSpaceContext(sharedSpaceId);
            OctaneConfiguration.SharedSpaceId = sharedSpaceContext.SharedSpaceId;
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
        }
    }
}
