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
using System.Collections.Generic;
using System.Windows.Media;

namespace MicroFocus.Adm.Octane.VisualStudio.Common
{
    /// <summary>
    /// Registry for general information regarding supported entity types
    /// </summary>
    public static class EntityTypeRegistry
    {
        private static readonly Dictionary<string, EntityTypeInformation> Registry = new Dictionary<string, EntityTypeInformation>
        {
            { WorkItem.SUBTYPE_DEFECT, new EntityTypeInformation(
                WorkItem.SUBTYPE_DEFECT, "Defect", "defect", "D", Color.FromRgb(190, 102, 92)) },

            { WorkItem.SUBTYPE_STORY, new EntityTypeInformation(
                WorkItem.SUBTYPE_STORY, "User Story", "user story", "US", Color.FromRgb(218, 199, 120)) },

            { WorkItem.SUBTYPE_QUALITY_STORY, new EntityTypeInformation(
                WorkItem.SUBTYPE_QUALITY_STORY, "Quality Story", "quality story", "QS", Color.FromRgb(95, 112, 118)) },

            { WorkItem.SUBTYPE_EPIC, new EntityTypeInformation(
                WorkItem.SUBTYPE_EPIC, "Epic", "epic", "E", Color.FromRgb(202, 170, 209)) },

            { WorkItem.SUBTYPE_FEATURE, new EntityTypeInformation(
                WorkItem.SUBTYPE_FEATURE, "Feature", "feature", "F", Color.FromRgb(226, 132, 90)) },

            { Test.SUBTYPE_MANUAL_TEST, new EntityTypeInformation(
                Test.SUBTYPE_MANUAL_TEST, "Manual Test", EntityTypeInformation.CommitMessageNotApplicable, "MT", Color.FromRgb(96, 121, 141)) },

            { TestGherkin.SUBTYPE_GHERKIN_TEST, new EntityTypeInformation(
                TestGherkin.SUBTYPE_GHERKIN_TEST, "Gherkin Test", EntityTypeInformation.CommitMessageNotApplicable, "GT", Color.FromRgb(120, 196, 192)) },

            { TestAutomated.SUBTYPE_TEST_AUTOMATED, new EntityTypeInformation(
                TestAutomated.SUBTYPE_TEST_AUTOMATED, "Automated Test", EntityTypeInformation.CommitMessageNotApplicable, "AT", Color.FromRgb(135, 123, 117)) },

            { TestSuite.SUBTYPE_TEST_SUITE, new EntityTypeInformation(
                TestSuite.SUBTYPE_TEST_SUITE, "Test Suite", EntityTypeInformation.CommitMessageNotApplicable, "TS", Color.FromRgb(133, 114, 147)) },

            { RunSuite.SUBTYPE_RUN_SUITE, new EntityTypeInformation(
                RunSuite.SUBTYPE_RUN_SUITE, "Test Suite Run", EntityTypeInformation.CommitMessageNotApplicable, "SR", Color.FromRgb(133, 169, 188)) },

            { RunManual.SUBTYPE_RUN_MANUAL, new EntityTypeInformation(
                RunManual.SUBTYPE_RUN_MANUAL, "Manual Test Run", EntityTypeInformation.CommitMessageNotApplicable, "MR", Color.FromRgb(133, 169, 188)) },

            { Requirement.SUBTYPE_DOCUMENT, new EntityTypeInformation(
                Requirement.SUBTYPE_DOCUMENT, "Requirement", EntityTypeInformation.CommitMessageNotApplicable, "R", Color.FromRgb(215, 194, 56)) },

            { Task.TYPE_TASK, new EntityTypeInformation(
                Task.TYPE_TASK, "Task",  Task.TYPE_TASK, "T", Color.FromRgb(137, 204, 174)) },

            { "comment", new EntityTypeInformation(
                "comment", "Comment", EntityTypeInformation.CommitMessageNotApplicable, "C", Color.FromRgb(234, 179, 124)) }
        };

        /// <summary>
        /// Return the associated entity type information based on the given entity
        /// </summary>
        public static EntityTypeInformation GetEntityTypeInformation(BaseEntity entity)
        {
            if (entity == null)
                return null;

            var entityType = Utility.GetConcreteEntityType(entity);

            EntityTypeInformation info;
            Registry.TryGetValue(entityType, out info);
            return info;
        }
    }
}
