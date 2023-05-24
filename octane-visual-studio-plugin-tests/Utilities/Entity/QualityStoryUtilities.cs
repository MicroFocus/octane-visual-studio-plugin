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
using MicroFocus.Adm.Octane.VisualStudio.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MicroFocus.Adm.Octane.VisualStudio.Tests.Utilities.Entity
{
    /// <summary>
    /// Utility class for managing <see cref="QualityStory"/>
    /// </summary>
    public static class QualityStoryUtilities
    {
        private static Phase _phaseNew;

        private static Phase GetPhaseNew()
        {
            if (_phaseNew == null)
            {
                _phaseNew = Utility.GetPhaseForEntityByLogicalName(WorkItem.SUBTYPE_QUALITY_STORY, "phase.quality_story.new");
            }

            return _phaseNew;
        }

        /// <summary>
        /// Create a new quality story entity
        /// </summary>
        public static QualityStory CreateQualityStory(string customName = null)
        {
            var name = customName ?? "QualityStory_" + Guid.NewGuid();
            var qualityStory = new QualityStory
            {
                Name = name,
                Phase = GetPhaseNew(),
                Parent = Utility.GetWorkItemRoot()
            };
            qualityStory.SetValue(CommonFields.Owner, BaseOctanePluginTest.User);

            var createQualityStory = BaseOctanePluginTest.EntityService.Create(BaseOctanePluginTest.WorkspaceContext, qualityStory, new[] { "name", "subtype" });
            Assert.AreEqual(name, createQualityStory.Name, "Newly created quality story doesn't have the expected name");
            Assert.IsFalse(string.IsNullOrEmpty(createQualityStory.Id), "Newly created quality story should have a valid ID");
            return createQualityStory;
        }
    }
}
