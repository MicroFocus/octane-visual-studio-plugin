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
using MicroFocus.Adm.Octane.VisualStudio.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace MicroFocus.Adm.Octane.VisualStudio.ViewModel
{
    /// <summary>
    /// View model representation for an entity field
    /// </summary>
    public class FieldViewModel : BaseItemViewModel
    {
        private readonly BaseEntity _parentEntity;
        private readonly string _emptyPlaceholder;
        private readonly Func<BaseEntity, object> _customContentFunc;

        private OctaneServices _octaneService;
        private List<BaseEntity> _referenceFieldContent;
        private List<BaseEntityWrapper> _referenceFieldContentName = new List<BaseEntityWrapper>();
        private Dispatcher uiDispatcher;
        private string _fieldEntity;
        private string logicalName;

        public List<BaseEntity> ReferenceFieldContentBaseEntity
        {
            get
            {
                return _referenceFieldContent;
            }
        }

        public FieldViewModel(BaseEntity entity, string fieldName, string fieldValue, bool isSelected) : base(entity)
        {

            _parentEntity = entity;
            Name = fieldName;
            Label = fieldValue;
            IsSelected = isSelected;
            uiDispatcher = Dispatcher.CurrentDispatcher;
        }

        public FieldViewModel(BaseEntity entity, FieldMetadata metadata, bool isSelected) : this(entity, metadata.Name, metadata.Label, isSelected)
        {
            Metadata = metadata;

            if (this.Metadata.FieldType == "reference")
            {
                List<String> targetAndLogicalName = getTargetAndLogicalName();
                try
                {
                    _fieldEntity = ReferenceEntityUtil.getApiEntityName(targetAndLogicalName[0]);
                } catch (Exception)
                {
                    FieldNotCompatible = true;
                    _fieldEntity = null;
                }
                
                logicalName = targetAndLogicalName[1];
            }
        }

        public FieldViewModel(BaseEntity entity, FieldInfo fieldInfo) : this(entity, fieldInfo.Name, fieldInfo.Title, false)
        {
            _emptyPlaceholder = fieldInfo.EmptyPlaceholder;
            _customContentFunc = fieldInfo.ContentFunc;
        }

        public FieldMetadata Metadata { get; }

        public string Label { get; }

        public string Name { get; }

        public bool HideLabel
        {
            get { return string.IsNullOrEmpty(Label); }
        }

        public bool IsSelected { get; set; }

        public async Task<List<FieldMetadata>> RetrieveEntityMetadata(BaseEntity entity)
        {
            List<FieldMetadata> myList = await FieldsMetadataService.GetFieldMetadata(entity);
            return myList;
        }

        public List<BaseEntityWrapper> ReferenceFieldContent
        {
            get
            {
                if (_referenceFieldContent == null)
                {
                    _octaneService = OctaneServices.GetInstance();

                    System.Threading.Tasks.Task taskRetrieveData = new System.Threading.Tasks.Task( async () =>
                    {
                        try
                        {
                            await _octaneService.Connect();
                            EntityListResult<BaseEntity> entities = _octaneService.GetEntitesReferenceFields(_fieldEntity);
                            if (_fieldEntity.Equals("sprints"))
                            {
                                _referenceFieldContent = getSprintFields(entities.data);
                            }
                            else if (_fieldEntity.Contains("list_node") && !string.IsNullOrEmpty(logicalName))
                            {
                                _referenceFieldContent = getListNodes(entities.data, logicalName);
                            }
                            else
                            {
                                _referenceFieldContent = entities.data;
                            }

                            if (_referenceFieldContent != null)
                            {
                                foreach (BaseEntity be in _referenceFieldContent)
                                {
                                    _referenceFieldContentName.Add(new BaseEntityWrapper(be));
                                }
                            }

                            uiDispatcher.Invoke(() =>
                            {
                                NotifyPropertyChanged("ReferenceFieldContent");
                                NotifyPropertyChanged("Content");
                            });
                        } catch (Exception)
                        {
                        }
                    });
                    taskRetrieveData.Start();
                }
                return _referenceFieldContentName;
            }
        }

        private List<BaseEntity> getListNodes(List<BaseEntity> data, string logicalName)
        {
            List<BaseEntity> referenceFieldContent = new List<BaseEntity>();

            foreach (BaseEntity be in data)
            {
                string currentItemsLogicalName = be.GetStringValue("logical_name");
                if (!string.IsNullOrEmpty(currentItemsLogicalName) && currentItemsLogicalName.Contains(logicalName))
                {
                    referenceFieldContent.Add(be);
                }
            }
            return referenceFieldContent;
        }

        private List<BaseEntity> getSprintFields(List<BaseEntity> data)
        {
            List<BaseEntity> referenceFieldContent = new List<BaseEntity>();
            BaseEntity parentsRelease = (BaseEntity)_parentEntity.GetValue("release");

            foreach (BaseEntity be in data)
            {
                BaseEntity sprintRelease = (BaseEntity)be.GetValue("release");
                if ( parentsRelease != null && sprintRelease.Name.Equals(parentsRelease.Name))
                {
                    referenceFieldContent.Add(be);
                }
            }
            return referenceFieldContent;
        }

        private List<String> getTargetAndLogicalName()
        {
            //first item is target, second one is logical name;
            BaseEntity fieldTypeData = Metadata.GetValue("field_type_data") as BaseEntity;
            ArrayList targets = new ArrayList();
            IsMultiple = (bool)fieldTypeData.GetValue("multiple");
            foreach (var elem in fieldTypeData.GetValue("targets") as ArrayList)
            {
                targets.Add(elem);
            }
            IsMoreThanOneTarget = false;
            if (targets.Count != 1)
            {
                IsMoreThanOneTarget = true;
            }
            IDictionary targetDictionary = targets[0] as IDictionary;
            string targetType = targetDictionary["type"] as string;
            string logical_name = targetDictionary["logical_name"] as string;

            List<String> result = new List<String>();
            result.Add(targetType);
            result.Add(logical_name);

            return result;

        }

        public bool FieldNotCompatible { get; set; }       

        public bool IsMultiple {get; set;}

        public bool IsMoreThanOneTarget { get; set; }

        public object Content
        {
            get
            {
                if (_customContentFunc != null)
                    return _customContentFunc(_parentEntity);


                var formattedValue = FieldsMetadataService.GetFormattedValue(_parentEntity, Name);
                if (formattedValue != null)
                    return formattedValue;

                object value = _parentEntity.GetValue(Name);
                switch (value)
                {
                    case null:
                        return _emptyPlaceholder;
                    case BaseEntity entity:
                        return entity.Name;
                    case EntityList<BaseEntity> entityList:
                        return FormatEntityList(entityList);
                    default:
                        return value;
                }
            }
            set
            {
                switch (Metadata.FieldType)
                {
                    case "boolean":
                        _parentEntity.SetValue(Name, value);
                        IsChanged = true;
                        break;
                    case "integer":
                        try
                        {
                            _parentEntity.SetIntValue(Name, int.Parse(value.ToString()));
                            IsChanged = true;
                        }
                        catch (Exception ex)
                        {
                            if (ex is FormatException || ex is OverflowException)
                            {
                                _parentEntity.SetValue(Name, "");
                                IsChanged = true;
                            }
                        }
                        break;
                    case "string":
                        _parentEntity.SetValue(Name, value.ToString());
                        IsChanged = true;
                        break;
                    case "date_time":
                        DateTime newValue = DateTime.MinValue;
                        if (value is DateTime)
                        {
                            newValue = (DateTime)value;
                            _parentEntity.SetDateTimeValue(Name, newValue.ToUniversalTime());
                        }
                        else
                        {
                            _parentEntity.SetDateTimeValue(Name, DateTime.UtcNow);
                        }
                        IsChanged = true;
                        break;
                    case "reference":
                        _parentEntity.SetValue(Name, ((BaseEntityWrapper) value).BaseEntity);
                        IsChanged = true;
                        break;
                }
            }
        }

        public bool IsChanged { get; private set; }

        private string FormatEntityList(EntityList<BaseEntity> value)
        {
            if (value.data.Count == 0)
            {
                return _emptyPlaceholder;
            }

            string[] entityNames = value.data.Select(x => x.Name).ToArray();
            return string.Join(", ", entityNames);
        }
    }

    public class BaseEntityWrapper
    {
        public BaseEntity BaseEntity { get; }

        public BaseEntityWrapper(BaseEntity entity)
        {
            BaseEntity = entity;
        }

        public override string ToString()
        {
            return BaseEntity.Name;
        }
    }
}
