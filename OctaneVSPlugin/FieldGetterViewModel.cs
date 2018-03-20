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
using MicroFocus.Adm.Octane.Api.Core.Services;
using System;
using System.Linq;

namespace MicroFocus.Adm.Octane.VisualStudio
{
    public class FieldGetterViewModel
    {
        private readonly BaseEntity _parentEntity;
        private readonly string _fieldName;
        private readonly string _fieldLabel;
        private readonly bool _isSelected;
        private readonly string emptyPlaceholder;
        private readonly Func<BaseEntity, object> customContentFunc;

        public FieldGetterViewModel(BaseEntity entity, string fieldName, string fieldValue, bool isSelected)
        {
            _parentEntity = entity;
            _fieldName = fieldName;
            _fieldLabel = fieldValue;
            IsSelected = isSelected;
        }

        public FieldGetterViewModel(BaseEntity entity, FieldInfo fieldInfo)
        {
            _parentEntity = entity;
            _fieldName = fieldInfo.Name;
            _fieldLabel = fieldInfo.Title;
            emptyPlaceholder = fieldInfo.EmptyPlaceholder;
            customContentFunc = fieldInfo.ContentFunc;
        }

        public string Label
        {
            get { return _fieldLabel; }
        }

        public string Name
        {
            get { return _fieldName; }
        }

        public bool HideLabel
        {
            get { return string.IsNullOrEmpty(Label); }
        }

        public bool IsSelected { get; set; }

        public object Content
        {
            get
            {
                if (customContentFunc != null)
                    return customContentFunc(_parentEntity);

                object value = _parentEntity.GetValue(_fieldName);
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
