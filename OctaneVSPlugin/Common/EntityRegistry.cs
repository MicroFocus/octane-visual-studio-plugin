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
    public static class EntityRegistry
    {
        private static readonly Dictionary<string, EntityInformation> Registry = new Dictionary<string, EntityInformation>
        {
            { WorkItem.SUBTYPE_DEFECT, new EntityInformation("Defect", "defect", "D", Color.FromRgb(190, 102, 92)) },
            { WorkItem.SUBTYPE_STORY, new EntityInformation("User Story", "user story", "US", Color.FromRgb(218, 199, 120)) },
            { WorkItem.SUBTYPE_QUALITY_STORY, new EntityInformation("Quality Story", "quality story", "QS", Color.FromRgb(95, 112, 118)) },
            { WorkItem.SUBTYPE_EPIC, new EntityInformation("Epic", "epic", "E", Color.FromRgb(202, 170, 209)) },
            { WorkItem.SUBTYPE_FEATURE, new EntityInformation("Feature", "feature", "F", Color.FromRgb(226, 132, 90)) },

            { Test.SUBTYPE_MANUAL_TEST, new EntityInformation("Manual Test", EntityInformation.CommitMessageNotApplicable, "MT", Color.FromRgb(96, 121, 141)) },
            { TestGherkin.SUBTYPE_GHERKIN_TEST, new EntityInformation("Gherkin Test", EntityInformation.CommitMessageNotApplicable, "GT", Color.FromRgb(120, 196, 192)) },
            { TestAutomated.SUBTYPE_TEST_AUTOMATED, new EntityInformation("Automated Test", EntityInformation.CommitMessageNotApplicable, "AT", Color.FromRgb(135, 123, 117)) },

            { TestSuite.SUBTYPE_TEST_SUITE, new EntityInformation("Test Suite", EntityInformation.CommitMessageNotApplicable, "TS", Color.FromRgb(133, 114, 147)) },
            { RunSuite.SUBTYPE_RUN_SUITE, new EntityInformation("Test Suite Run", EntityInformation.CommitMessageNotApplicable, "SR", Color.FromRgb(133, 169, 188)) },
            { RunManual.SUBTYPE_RUN_MANUAL, new EntityInformation("Manual Test Run", EntityInformation.CommitMessageNotApplicable, "MR", Color.FromRgb(133, 169, 188)) },

            { Requirement.SUBTYPE_DOCUMENT, new EntityInformation("Requirement", EntityInformation.CommitMessageNotApplicable, "R", Color.FromRgb(215, 194, 56)) },

            { Task.TYPE_TASK, new EntityInformation("Task",  Task.TYPE_TASK, "T", Color.FromRgb(137, 204, 174)) },

            { "comment", new EntityInformation("Comment", EntityInformation.CommitMessageNotApplicable, "C", Color.FromRgb(234, 179, 124)) }
        };

        public static EntityInformation GetEntityInformation(BaseEntity entity)
        {
            if (entity == null)
                return null;

            var entityType = Utility.GetConcreteEntityType(entity);

            EntityInformation info;
            Registry.TryGetValue(entityType, out info);
            return info;
        }
    }

    public class EntityInformation
    {
        /// <summary>
        /// Placeholder value for entities that should not support copy of commit message.
        /// </summary>
        public const string CommitMessageNotApplicable = "";

        public string DisplayName { get; }
        public string CommitMessage { get; }
        public string ShortLabel { get; }
        public Color LabelColor { get; }

        public EntityInformation(string displayName, string commitMessage, string shortLabel, Color labelColor)
        {
            DisplayName = displayName;
            CommitMessage = commitMessage;
            ShortLabel = shortLabel;
            LabelColor = labelColor;
        }

        internal bool IsCopyCommitMessageSupported
        {
            get { return CommitMessageNotApplicable != CommitMessage; }
        }
    }
}
