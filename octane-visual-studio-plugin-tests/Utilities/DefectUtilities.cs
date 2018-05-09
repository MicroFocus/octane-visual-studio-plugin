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
using MicroFocus.Adm.Octane.Api.Core.Services;
using MicroFocus.Adm.Octane.Api.Core.Services.Query;
using MicroFocus.Adm.Octane.Api.Core.Services.RequestContext;
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

        private static Phase GetPhaseNew(EntityService entityService, WorkspaceContext workspaceContext)
        {
            if (_phaseNew == null)
            {
                _phaseNew = Utility.GetPhaseForEntityByLogicalName(entityService, workspaceContext,
                    WorkItem.SUBTYPE_DEFECT, "phase.defect.new");
            }

            return _phaseNew;
        }

        private static ListNode GetSeverityByName(EntityService entityService, WorkspaceContext workspaceContext, string name)
        {
            var suffix = name.ToLower().Replace(" ", "_");
            var logicalName = "list_node.severity." + suffix;
            var queryPhrases = new List<QueryPhrase>
            {
                new LogicalQueryPhrase(BaseEntity.LOGICAL_NAME_FIELD, logicalName)
            };

            var fields = new List<string>() { BaseEntity.NAME_FIELD, BaseEntity.LOGICAL_NAME_FIELD };

            var result = entityService.Get<ListNode>(workspaceContext, queryPhrases, fields);
            Assert.AreEqual(1, result.total_count, $"There should only be one severity with the logical name \"{logicalName }\"");
            return result.data[0];
        }

        /// <summary>
        /// Create a new defect entity
        /// </summary>
        public static Defect CreateDefect(EntityService entityService, WorkspaceContext workspaceContext, string customName = null)
        {
            var name = customName ?? "Defect_" + Guid.NewGuid();
            var defect = new Defect
            {
                Name = name,
                Phase = GetPhaseNew(entityService, workspaceContext),
                Severity = GetSeverityByName(entityService, workspaceContext, "High"),
                Parent = Utility.GetWorkItemRoot(entityService, workspaceContext)
            };

            var createdDefect = entityService.Create(workspaceContext, defect, new[] { Defect.NAME_FIELD, Defect.AUTHOR_FIELD });
            Assert.AreEqual(name, createdDefect.Name, "Mismatched defect name");

            return createdDefect;
        }
    }
}
