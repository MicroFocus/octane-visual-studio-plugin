using Hpe.Nga.Api.Core.Entities;

namespace Hpe.Nga.Octane.VisualStudio
{
    public class FieldGetterViewModel
    {
        private readonly OctaneItemViewModel itemViewModel;
        private readonly string fieldName;
        private readonly string fieldLabel;

        public FieldGetterViewModel(OctaneItemViewModel itemViewModel, string fieldName, string fieldLabel)
        {
            this.itemViewModel = itemViewModel;
            this.fieldName = fieldName;
            this.fieldLabel = fieldLabel;
        }

        public string Label
        {
            get { return fieldLabel; }
        }

        public object Content
        {
            get
            {
                object value = itemViewModel.WorkItem.GetValue(fieldName);
                if (value is BaseEntity)
                {
                    return ((BaseEntity)value).Name;
                }
                else
                {
                    return value;
                }
            }
        }
    }
}
