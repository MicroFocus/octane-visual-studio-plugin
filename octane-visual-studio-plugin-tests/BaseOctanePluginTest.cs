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

using MicroFocus.Adm.Octane.Api.Core.Tests;
using MicroFocus.Adm.Octane.VisualStudio.Common;
using MicroFocus.Adm.Octane.VisualStudio.Tests.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace MicroFocus.Adm.Octane.VisualStudio.Tests
{
    /// <summary>
    /// Base test class for plugin tests
    /// </summary>
    public abstract class BaseOctanePluginTest : BaseTest
    {
        protected readonly MyWorkMetadata MyWorkMetadata = new MyWorkMetadata();
        private dynamic _persistedFieldsCache;

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            InitConnection(context);

            OctaneConfiguration.Url = host;
            OctaneConfiguration.Username = userName;
            OctaneConfiguration.Password = password;
            OctaneConfiguration.WorkSpaceId = workspaceContext.WorkspaceId;
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
        }

        [TestCleanup]
        public void TestCleanup()
        {
            ExposedClass.From(typeof(FieldsCache))._persistedFieldsCache = _persistedFieldsCache;
            ExposedClass.From(typeof(FieldsCache)).PersistFieldsMetadata();
        }
    }
}
