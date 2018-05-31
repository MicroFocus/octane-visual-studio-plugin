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
using MicroFocus.Adm.Octane.VisualStudio.Common;
using System;
using System.Globalization;
using System.Text;

namespace MicroFocus.Adm.Octane.VisualStudio.ViewModel
{
    /// <summary>
    /// View model for a comment
    /// </summary>
    public class CommentViewModel : OctaneItemViewModel
    {
        private readonly Comment _commentEntity;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="entity"></param>
        public CommentViewModel(BaseEntity entity)
            : base(entity)
        {
            _commentEntity = (Comment)entity;
            ParentEntity = GetOwnerEntity();

            Author = Utility.GetAuthorFullName(_commentEntity);
            CreationTime = DateTime.Parse(_commentEntity.CreationTime).ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            Text = Utility.StripHtml(_commentEntity.Text);
            OriginalText = _commentEntity.Text;
        }

        /// <inheritdoc/>>
        public override bool VisibleID { get { return false; } }

        /// <inheritdoc/>>
        public override string Title
        {
            get
            {
                if (ParentEntity == null)
                    return "Unable to determine the comment's owner";

                var parentEntityTypeInformation = EntityTypeRegistry.GetEntityTypeInformation(ParentEntity);
                if (parentEntityTypeInformation == null)
                    return string.Empty;

                var sb = new StringBuilder("Comment on ")
                    .Append(parentEntityTypeInformation.DisplayName.ToLower())
                    .Append(": ")
                    .Append(ParentEntity.Id.ToString())
                    .Append(" ")
                    .Append(ParentEntity.Name);
                return sb.ToString();
            }
        }

        /// <summary>
        /// Returns entity under which the comment is located
        /// </summary>
        public BaseEntity ParentEntity { get; }

        private BaseEntity GetOwnerEntity()
        {
            if (_commentEntity.OwnerWorkItem != null)
                return _commentEntity.OwnerWorkItem;

            if (_commentEntity.OwnerTest != null)
                return _commentEntity.OwnerTest;

            if (_commentEntity.OwnerRun != null)
                return _commentEntity.OwnerRun;

            return _commentEntity.OwnerRequirement;
        }

        /// <summary>
        /// Returns person who posted the comment
        /// </summary>
        public string Author { get; }

        /// <summary>
        /// Return the time the comment was created
        /// </summary>
        public string CreationTime { get; }

        /// <summary>
        /// Receives comment text with stripped HTML tags
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Returns original text as it was received from Octane
        /// </summary>
        public string OriginalText { get; }
    }
}
