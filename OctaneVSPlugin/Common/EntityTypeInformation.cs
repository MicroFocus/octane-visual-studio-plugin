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

using System.Windows.Media;

namespace MicroFocus.Adm.Octane.VisualStudio.Common
{
    /// <summary>
    /// General information about an entity type
    /// </summary>
    public class EntityTypeInformation
    {
        /// <summary>
        /// Placeholder value for entities that should not support copy of commit message.
        /// </summary>
        public const string CommitMessageNotApplicable = "";

        /// <summary>
        /// Concrete type name
        /// </summary>
        public string EntityTypeName { get; }

        /// <summary>
        /// Type display name
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Commit header associated with current entity type
        /// </summary>
        public string CommitMessage { get; }

        /// <summary>
        /// Type abreviation
        /// </summary>
        public string ShortLabel { get; }

        /// <summary>
        /// Type color
        /// </summary>
        public Color Color { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public EntityTypeInformation(string entityTypeName, string displayName, string commitMessage, string shortLabel, Color color)
        {
            EntityTypeName = entityTypeName;
            DisplayName = displayName;
            CommitMessage = commitMessage;
            ShortLabel = shortLabel;
            Color = color;
        }

        /// <summary>
        /// Flag specifying whether the current entity type supports a commit message
        /// </summary>
        internal bool IsCopyCommitMessageSupported
        {
            get { return CommitMessageNotApplicable != CommitMessage; }
        }
    }
}
