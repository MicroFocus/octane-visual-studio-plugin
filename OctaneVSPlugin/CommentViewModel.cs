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
using System.Text;

namespace MicroFocus.Adm.Octane.VisualStudio
{
    public class CommentViewModel : OctaneItemViewModel
    {
        private readonly Comment commentEntity;

        public CommentViewModel(BaseEntity entity, MyWorkMetadata myWorkMetadata)
            : base(entity, myWorkMetadata)
        {
            commentEntity = (Comment)entity;
            ParentEntity = GetOwnerEntity();
        }

        public override bool VisibleID { get { return false; } }

        public override string Title
        {
            get
            {
                if (ParentEntity == null)
                    return "Unable to determine the comment's owner entity";

                var sb = new StringBuilder("Comment on ")
                    .Append(EntityNames.GetDisplayName(ParentEntity.TypeName).ToLower())
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
            if (commentEntity.OwnerWorkItem != null)
                return commentEntity.OwnerWorkItem;

            if (commentEntity.OwnerTest != null)
                return commentEntity.OwnerTest;

            if (commentEntity.OwnerRun != null)
                return commentEntity.OwnerRun;

            return commentEntity.OwnerRequirement;
        }
    }
}
