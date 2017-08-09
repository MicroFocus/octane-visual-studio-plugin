using System.Linq;
using Hpe.Nga.Api.Core.Entities;
using System.Collections.Generic;
using System.Windows.Media;

namespace Hpe.Nga.Octane.VisualStudio
{
    public class OctaneItemViewModel
    {
        private readonly BaseEntity entity;
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

        public long ID { get { return entity.Id; } }
        public string Name { get { return entity.Name; } }

        public string SubType
        {
            get { return entity.GetStringValue(WorkItemFields.SUB_TYPE); }
        }

        public string Description
        {
            get { return entity.GetStringValue(WorkItemFields.DESCRIPTION) ?? string.Empty; }
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
