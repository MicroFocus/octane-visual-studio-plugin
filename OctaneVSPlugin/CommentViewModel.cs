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
        }

        public override bool VisibleID { get { return false; } }

        public override string Title
        {
            get
            {
                BaseEntity owner = GetOwnerEntity();
                if (owner == null)
                    return "Orphaned comment";

                var sb = new StringBuilder("Comment on ")
                    .Append(owner.TypeName)
                    .Append(": ")
                    .Append(owner.Id.ToString())
                    .Append(" ")
                    .Append(owner.Name);
                return sb.ToString();
            }
        }

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
