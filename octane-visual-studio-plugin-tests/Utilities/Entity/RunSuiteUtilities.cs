/*!
* © Copyright 2017-2022 Micro Focus or one of its affiliates.
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MicroFocus.Adm.Octane.VisualStudio.Tests.Utilities.Entity
{
    /// <summary>
    /// Utility class for managing <see cref="RunSuite"/> entities
    /// </summary>
    public static class RunSuiteUtilities
    {
        /// <summary>
        /// Create a new suite run entity
        /// </summary>
        public static RunSuite CreateSuiteRun(TestSuite parent, string customName = null)
        {
            var name = customName ?? "SuiteRun_" + Guid.NewGuid();

            var status = new BaseEntity();
            status.SetValue("id", "list_node.run_native_status.not_completed");
            status.SetValue("type", "list_node");

            var suiteRun = new RunSuite
            {
                Name = name,
                DefaultRunBy = BaseOctanePluginTest.User,
                Release = BaseOctanePluginTest.CurrentRelease,
                Parent = parent,
                NativeStatus = status
            };

            var createdSuiteRun = BaseOctanePluginTest.EntityService.Create(BaseOctanePluginTest.WorkspaceContext, suiteRun, new[] { "name", "subtype" });
            Assert.AreEqual(name, createdSuiteRun.Name, "Mismatched name for newly created suite run");
            Assert.IsTrue(!string.IsNullOrEmpty(createdSuiteRun.Id), "Suite run id shouldn't be null or empty");
            return createdSuiteRun;
        }
    }
}
