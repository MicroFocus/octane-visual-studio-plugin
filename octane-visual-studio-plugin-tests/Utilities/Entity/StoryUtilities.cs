﻿/*!
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
    /// Utility class for managing <see cref="Story"/>
    /// </summary>
    public static class StoryUtilities
    {
        private static Phase _phaseNew;

        private static Phase GetPhaseNew()
        {
            if (_phaseNew == null)
            {
                _phaseNew = Utility.GetPhaseForEntityByLogicalName(WorkItem.SUBTYPE_STORY, "phase.story.new");
            }

            return _phaseNew;
        }

        /// <summary>
        /// Create a new user story entity
        /// </summary>
        public static Story CreateStory(string customName = null, bool setOwner = true)
        {
            var name = customName ?? "Story_" + Guid.NewGuid();
            var story = new Story
            {
                Name = name,
                Phase = GetPhaseNew(),
                Parent = Utility.GetWorkItemRoot()
            };
            if(setOwner)
            {
                story.SetValue(CommonFields.Owner, BaseOctanePluginTest.User);
            }
            
            var createdStory = BaseOctanePluginTest.EntityService.Create(BaseOctanePluginTest.WorkspaceContext, story, new[] { "name", "subtype" });
            Assert.AreEqual(name, createdStory.Name, "Newly created story doesn't have the expected name");
            Assert.IsFalse(string.IsNullOrEmpty(createdStory.Id), "Newly created story should have a valid ID");
            return createdStory;
        }
    }
}
