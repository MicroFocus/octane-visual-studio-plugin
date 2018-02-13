using MicroFocus.Adm.Octane.Api.Core.Entities;
using MicroFocus.Adm.Octane.Api.Core.Entities.WorkItems;
using System.Text;

namespace Hpe.Nga.Octane.VisualStudio
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
                    return "Orphaned comment";

                var sb = new StringBuilder("Comment on ")
                    .Append(ParentEntity.TypeName)
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

            return null;
        }
    }
}
