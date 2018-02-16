using MicroFocus.Adm.Octane.Api.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace MicroFocus.Adm.Octane.VisualStudio
{
    public class OctaneItemViewModel
    {
        protected readonly BaseEntity entity;
        private readonly MyWorkMetadata myWorkMetadata;

        private readonly List<FieldGetterViewModel> topFields;
        private readonly List<FieldGetterViewModel> bottomFields;
        private readonly FieldGetterViewModel subTitleField;

        public OctaneItemViewModel(BaseEntity entity, MyWorkMetadata myWorkMetadata)
        {
            this.entity = entity;
            this.myWorkMetadata = myWorkMetadata;

            topFields = new List<FieldGetterViewModel>();
            bottomFields = new List<FieldGetterViewModel>();

            subTitleField = new FieldGetterViewModel(this, myWorkMetadata.GetSubTitleFieldInfo(entity));

            foreach (FieldInfo fieldInfo in myWorkMetadata.GetTopFieldsInfo(entity))
            {
                topFields.Add(new FieldGetterViewModel(this, fieldInfo));
            }

            foreach (FieldInfo fieldInfo in myWorkMetadata.GetBottomFieldsInfo(entity))
            {
                bottomFields.Add(new FieldGetterViewModel(this, fieldInfo));
            }
        }

        public BaseEntity Entity { get { return entity; } }

        public EntityId ID { get { return entity.Id; } }

        public virtual bool VisibleID { get { return true; } }

        public virtual string Title { get { return entity.Name; } }

        public string TypeName
        {
            get { return entity.TypeName; }
        }

        public string SubType
        {
            get { return entity.GetStringValue(CommonFields.SUB_TYPE); }
        }

        public string Description
        {
            get { return entity.GetStringValue(CommonFields.DESCRIPTION) ?? string.Empty; }
        }

        public string CommitMessage
        {
            get
            {
                string message = string.Format("{0} #{1}: ", myWorkMetadata.GetCommitMessageTypeName(entity), ID);
                return message;
            }
        }

        public bool IsSupportCopyCommitMessage
        {
            get { return myWorkMetadata.IsSupportCopyCommitMessage(entity); }
        }

        public FieldGetterViewModel SubTitleField
        {
            get { return subTitleField; }
        }

        public IEnumerable<object> TopFields
        {
            get
            {
                return FieldsWithSeparators(topFields);
            }
        }
        public IEnumerable<object> BottomFields
        {
            get
            {
                return FieldsWithSeparators(bottomFields);
            }
        }

        private IEnumerable<object> FieldsWithSeparators(List<FieldGetterViewModel> fields)
        {
            // Handle the case there are no fields so we don't need any seperators.
            if (fields.Count == 0)
            {
                yield break;
            }

            foreach (FieldGetterViewModel field in fields.Take(fields.Count - 1))
            {
                yield return field;
                yield return SeparatorViewModel.Make();
            }

            yield return fields.Last();
        }

        public string IconText
        {
            get
            {
                string iconText = myWorkMetadata.GetIconText(entity);
                return iconText;
            }
        }

        public Color IconBackgroundColor
        {
            get
            {
                Color bgc = myWorkMetadata.GetIconColor(entity);
                return bgc;
            }
        }
    }
}
