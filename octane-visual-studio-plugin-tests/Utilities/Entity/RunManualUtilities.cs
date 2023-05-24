/*!
* Copyright 2017 – 2023 Open Text.
*
* The only warranties for products and services of Open Text and
* its affiliates and licensors (“Open Text”) are as may be set
* forth in the express warranty statements accompanying such products
* and services. Nothing herein should be construed as constituting an
* additional warranty. Open Text shall not be liable for technical or
* editorial errors or omissions contained herein. The information
* contained herein is subject to change without notice.
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
    /// Utility class for managing <see cref="RunManual"/> entities
    /// </summary>
    public static class RunManualUtilities
    {
        /// <summary>
        /// Create a new manual run entity
        /// </summary>
        public static RunManual CreateManualRun(TestManual parent, string customName = null)
        {
            var name = customName ?? "ManualRun_" + Guid.NewGuid();

            var status = new BaseEntity();
            status.SetValue("id", "list_node.run_native_status.not_completed");
            status.SetValue("type", "list_node");

            var manualRun = new RunManual
            {
                Name = name,
                RunBy = BaseOctanePluginTest.User,
                Release = BaseOctanePluginTest.CurrentRelease,
                Parent = parent,
                NativeStatus = status
            };

            var createdManualRun = BaseOctanePluginTest.EntityService.Create(BaseOctanePluginTest.WorkspaceContext, manualRun, new[] { "name", "subtype" });
            Assert.AreEqual(name, createdManualRun.Name, "Mismatched name for newly created manual run");
            Assert.IsTrue(!string.IsNullOrEmpty(createdManualRun.Id), "Manual run id shouldn't be null or empty");
            return createdManualRun;
        }
    }
}
