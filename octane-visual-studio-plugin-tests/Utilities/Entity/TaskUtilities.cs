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
    /// Utility class for managing <see cref="Task"/>
    /// </summary>
    public static class TaskUtilities
    {
        /// <summary>
        /// Create a new task entity
        /// </summary>
        public static Task CreateTask(WorkItem parent, string customName = null)
        {
            var name = customName ?? "Task_" + Guid.NewGuid();
            var task = new Task
            {
                Name = name,
                Story = parent
            };
            task.SetValue(CommonFields.Owner, BaseOctanePluginTest.User);

            var createdTask = BaseOctanePluginTest.EntityService.Create(BaseOctanePluginTest.WorkspaceContext, task, new[] { "name", Task.STORY_FIELD });
            Assert.AreEqual(name, createdTask.Name, "Newly created task doesn't have the expected name");
            Assert.IsFalse(string.IsNullOrEmpty(createdTask.Id), "Newly created task should have a valid ID");
            return createdTask;
        }
    }
}
