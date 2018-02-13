using MicroFocus.Adm.Octane.Api.Core.Entities;
using MicroFocus.Adm.Octane.Api.Core.Services;
using System;
using System.Linq;

namespace Hpe.Nga.Octane.VisualStudio
{
    public class FieldGetterViewModel
    {
        private readonly OctaneItemViewModel itemViewModel;
        private readonly string fieldName;
        private readonly string fieldLabel;
        private readonly string emptyPlaceholder;
        private readonly Func<BaseEntity, object> customContentFunc;

        public FieldGetterViewModel(OctaneItemViewModel itemViewModel, FieldInfo fieldInfo)
        {
            this.itemViewModel = itemViewModel;
            fieldName = fieldInfo.Name;
            fieldLabel = fieldInfo.Title;
            emptyPlaceholder = fieldInfo.EmptyPlaceholder;
            customContentFunc = fieldInfo.ContentFunc;
        }

        public string Label
        {
            get { return fieldLabel; }
        }

        public bool HideLabel
        {
            get { return string.IsNullOrEmpty(Label); }
        }

        public object Content
        {
            get
            {
                if (customContentFunc != null)
                    return customContentFunc(itemViewModel.Entity);

                object value = itemViewModel.Entity.GetValue(fieldName);
                switch (value)
                {
                    case null:
                        return emptyPlaceholder;
                    case BaseEntity entity:
                        return entity.Name;
                    case EntityList<BaseEntity> entityList:
                        return FormatEntityList(entityList);
                    default:
                        return value;
                }
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
