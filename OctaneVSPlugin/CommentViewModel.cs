using MicroFocus.Adm.Octane.Api.Core.Entities;

namespace Hpe.Nga.Octane.VisualStudio
{
    public class CommentViewModel : OctaneItemViewModel
    {
        public CommentViewModel(BaseEntity entity, MyWorkMetadata myWorkMetadata)
            : base(entity, myWorkMetadata)
        {
        }

        public override bool VisibleID { get { return false; } }

        public override string Title { get { return entity.TypeName; } }
    }
}
