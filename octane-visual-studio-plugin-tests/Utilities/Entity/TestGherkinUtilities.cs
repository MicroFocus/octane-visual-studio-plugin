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

using MicroFocus.Adm.Octane.Api.Core.Entities;
using MicroFocus.Adm.Octane.VisualStudio.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MicroFocus.Adm.Octane.VisualStudio.Tests.Utilities.Entity
{
    /// <summary>
    /// Utility class for managing <see cref="TestGherkin"/> entities
    /// </summary>
    public static class TestGherkinUtilities
    {
        private static Phase _phaseNew;

        private static Phase GetPhaseNew()
        {
            if (_phaseNew == null)
            {
                _phaseNew = Utility.GetPhaseForEntityByLogicalName(TestGherkin.SUBTYPE_GHERKIN_TEST, "phase.gherkin_test.new");
            }

            return _phaseNew;
        }

        /// <summary>
        /// Create a new gherkin test entity
        /// </summary>
        public static TestGherkin CreateGherkinTest(string customName = null)
        {
            var name = customName ?? "GherkinTest_" + Guid.NewGuid();
            var test = new TestGherkin
            {
                Name = name,
                Phase = GetPhaseNew()
            };
            test.SetValue(CommonFields.Owner, BaseOctanePluginTest.User);

            var createdTest = BaseOctanePluginTest.EntityService.Create(BaseOctanePluginTest.WorkspaceContext, test, new[] { "name", "subtype" });
            Assert.AreEqual(name, createdTest.Name, "Mismatched name for newly created gherkin test");
            Assert.IsTrue(!string.IsNullOrEmpty(createdTest.Id), "Gherking test id shouldn't be null or empty");
            return createdTest;
        }
    }
}
