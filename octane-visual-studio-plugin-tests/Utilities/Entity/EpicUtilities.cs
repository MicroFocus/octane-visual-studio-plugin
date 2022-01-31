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
using MicroFocus.Adm.Octane.VisualStudio.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MicroFocus.Adm.Octane.VisualStudio.Tests.Utilities.Entity
{
    /// <summary>
    /// Utility class for managing <see cref="Epic"/> entities
    /// </summary>
    public static class EpicUtilities
    {
        private static Phase _phaseNew;

        private static Phase GetPhaseNew()
        {
            if (_phaseNew == null)
            {
                _phaseNew = Utility.GetPhaseForEntityByLogicalName(WorkItem.SUBTYPE_EPIC, "phase.epic.new");
            }

            return _phaseNew;
        }

        /// <summary>
        /// Create a new epic entity
        /// </summary>
        public static Epic CreateEpic(string customName = null)
        {
            var epicName = customName ?? "Epic_" + Guid.NewGuid();
            var epicToCreate = new Epic
            {
                Name = epicName,
                Phase = GetPhaseNew(),
                Parent = Utility.GetWorkItemRoot()
            };
            epicToCreate.SetValue(CommonFields.Owner, BaseOctanePluginTest.User);

            var createdEpic = BaseOctanePluginTest.EntityService.Create(BaseOctanePluginTest.WorkspaceContext, epicToCreate, new[] { "name", "subtype" });
            Assert.AreEqual(epicName, createdEpic.Name, "Mismatched epic name");

            return createdEpic;
        }
    }
}
