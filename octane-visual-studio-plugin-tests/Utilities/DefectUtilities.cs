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
using MicroFocus.Adm.Octane.Api.Core.Services.Query;
using MicroFocus.Adm.Octane.VisualStudio.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace MicroFocus.Adm.Octane.VisualStudio.Tests.Utilities
{
    /// <summary>
    /// Utility class for managing <see cref="Defect"/> entities
    /// </summary>
    public static class DefectUtilities
    {
        private static Phase _phaseNew;

        private static Phase GetPhaseNew()
        {
            if (_phaseNew == null)
            {
                _phaseNew = Utility.GetPhaseForEntityByLogicalName(WorkItem.SUBTYPE_DEFECT, "phase.defect.new");
            }

            return _phaseNew;
        }

        private static ListNode GetSeverityByName(string name)
        {
            var suffix = name.ToLower().Replace(" ", "_");
            var logicalName = "list_node.severity." + suffix;
            var queryPhrases = new List<QueryPhrase>
            {
                new LogicalQueryPhrase(BaseEntity.LOGICAL_NAME_FIELD, logicalName)
            };

            var fields = new List<string>() { BaseEntity.NAME_FIELD, BaseEntity.LOGICAL_NAME_FIELD };

            var result = BaseOctanePluginTest.EntityService.Get<ListNode>(BaseOctanePluginTest.WorkspaceContext, queryPhrases, fields);
            Assert.AreEqual(1, result.total_count, $"There should only be one severity with the logical name \"{logicalName }\"");
            return result.data[0];
        }

        /// <summary>
        /// Create a new defect entity
        /// </summary>
        public static Defect CreateDefect(string customName = null)
        {
            var name = customName ?? "Defect_" + Guid.NewGuid();
            var defect = new Defect
            {
                Name = name,
                Phase = GetPhaseNew(),
                Severity = GetSeverityByName("High"),
                Parent = Utility.GetWorkItemRoot()
            };
            defect.SetValue(CommonFields.Owner, BaseOctanePluginTest.User);

            var createdDefect = BaseOctanePluginTest.EntityService.Create(BaseOctanePluginTest.WorkspaceContext, defect, new[] { Defect.NAME_FIELD, Defect.AUTHOR_FIELD });
            Assert.AreEqual(name, createdDefect.Name, "Mismatched defect name");

            return createdDefect;
        }
    }
}
