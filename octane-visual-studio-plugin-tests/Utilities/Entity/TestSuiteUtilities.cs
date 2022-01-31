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
    /// Utility class for managing <see cref="TestSuite"/> entities
    /// </summary>
    public static class TestSuiteUtilities
    {
        /// <summary>
        /// Create a new test suite entity
        /// </summary>
        public static TestSuite CreateTestSuite(string customName = null)
        {
            var name = customName ?? "TestSuite_" + Guid.NewGuid();
            var testSuite = new TestSuite
            {
                Name = name,
            };

            var createTestSuite = BaseOctanePluginTest.EntityService.Create(BaseOctanePluginTest.WorkspaceContext, testSuite, new[] { "name", "subtype" });
            Assert.AreEqual(name, createTestSuite.Name, "Mismatched name for newly created test suite");
            Assert.IsTrue(!string.IsNullOrEmpty(createTestSuite.Id), "Test suite id shouldn't be null or empty");
            return createTestSuite;
        }
    }
}
