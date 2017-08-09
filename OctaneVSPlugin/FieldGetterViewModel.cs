using System.Linq;
using Hpe.Nga.Api.Core.Entities;
using Hpe.Nga.Api.Core.Services;

namespace Hpe.Nga.Octane.VisualStudio
{
    public class FieldGetterViewModel
    {
        private readonly OctaneItemViewModel itemViewModel;
        private readonly string fieldName;
        private readonly string fieldLabel;
        private readonly string emptyPlaceholder;

        public FieldGetterViewModel(OctaneItemViewModel itemViewModel, FieldInfo fieldInfo) 
        {
            this.itemViewModel = itemViewModel;
            fieldName = fieldInfo.Name;
            fieldLabel = fieldInfo.Title;
            emptyPlaceholder = fieldInfo.EmptyPlaceholder;
        }

        public string Label
        {
            get { return fieldLabel; }
        }

        public object Content
        {
            get
            {
                object value = itemViewModel.Entity.GetValue(fieldName);
                if (value == null)
                {
                    return emptyPlaceholder;
                }

                if (value is BaseEntity)
                {
                    return ((BaseEntity)value).Name;
                }

                if (value is EntityList<BaseEntity>)
                {
                    return FormatEntityList((EntityList<BaseEntity>)value);
                }

                return value;
            }
        }

        private string FormatEntityList(EntityList<BaseEntity> value)
        {
            if (value.data.Count == 0)
            {
                return emptyPlaceholder;
            }

            string[] entityNames = value.data.Select(x => x.Name).ToArray();
            return string.Join(", ", entityNames);
        }
    }
}
