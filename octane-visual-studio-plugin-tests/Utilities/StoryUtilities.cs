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
    /// Utility class for managing <see cref="Story"/>
    /// </summary>
    public static class StoryUtilities
    {
        private static Phase _phaseNew;
        private static WorkItemRoot _workItemRoot;

        private static Phase GetPhaseForEntityByLogicalName(EntityService entityService, WorkspaceContext workspaceContext, String entityTypeName, String logicalName)
        {
            var byEntityPhrase = new LogicalQueryPhrase(Phase.ENTITY_FIELD, entityTypeName);
            var byNamePhrase = new LogicalQueryPhrase(Phase.LOGICAL_NAME_FIELD, logicalName);
            var queryPhrases = new List<QueryPhrase> { byEntityPhrase, byNamePhrase };

            EntityListResult<Phase> result = entityService.Get<Phase>(workspaceContext, queryPhrases, null);
            Assert.AreEqual(1, result.total_count, $"There should only be one phase with the logical name \"{logicalName }\" for type \"{entityService}\"");
            return result.data[0];
        }

        private static WorkItemRoot GetWorkItemRoot(EntityService entityService, WorkspaceContext workspaceContext)
        {
            if (_workItemRoot == null)
            {
                List<String> fields = new List<String>() { Phase.NAME_FIELD };
                EntityListResult<WorkItemRoot> result = entityService.Get<WorkItemRoot>(workspaceContext, null, fields);
                Assert.AreEqual(1, result.total_count);
                _workItemRoot = result.data[0];
            }

            return _workItemRoot;
        }

        private static Phase GetPhaseNew(EntityService entityService, WorkspaceContext workspaceContext)
        {
            if (_phaseNew == null)
                _phaseNew = GetPhaseForEntityByLogicalName(entityService, workspaceContext, WorkItem.SUBTYPE_STORY, "phase.story.new");

            return _phaseNew;
        }

        /// <summary>
        /// Create a new user story entity
        /// </summary>
        public static Story CreateStory(EntityService entityService, WorkspaceContext workspaceContext, string customName = null)
        {
            var name = customName ?? "Story_" + Guid.NewGuid();
            var story = new Story
            {
                Name = name,
                Phase = GetPhaseNew(entityService, workspaceContext),
                Parent = GetWorkItemRoot(entityService, workspaceContext)
            };

            var createdStory = entityService.Create(workspaceContext, story, new[] { "name", "subtype" });
            Assert.AreEqual(name, createdStory.Name, "Newly created story doesn't have the expected name");
            Assert.IsFalse(string.IsNullOrEmpty(createdStory.Id), "Newly created story should have a valid ID");
            return createdStory;
        }
    }
}
