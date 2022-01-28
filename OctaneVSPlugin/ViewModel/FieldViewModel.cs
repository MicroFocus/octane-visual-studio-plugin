/*!
* © Copyright 2017-2022 Micro Focus or one of its affiliates.
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
using MicroFocus.Adm.Octane.Api.Core.Services.Query;
using MicroFocus.Adm.Octane.VisualStudio.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
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
        private Dispatcher uiDispatcher;
        private string _fieldEntity;
        private string logicalName;

        public event EventHandler ChangeHandler;
        private void OnValueChanged(EventArgs e)
        {
            ChangeHandler?.Invoke(this, e);
        }

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
            MakeValueNull = new DelegatedCommand(SetMakeValueNull);
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
                }
                catch (Exception)
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

        public async Task<List<BaseEntityWrapper>> getRelatedEntities(string filterString)
        {
            List<BaseEntityWrapper> results = new List<BaseEntityWrapper>();
            if (Metadata.FieldType.Equals("boolean"))
            {
                BaseEntity falseBaseEntity = new BaseEntity();
                falseBaseEntity.Name = "False";


                BaseEntity trueBaseEntity = new BaseEntity();
                trueBaseEntity.Name = "True";

                results.Add(new BaseEntityWrapper(falseBaseEntity));
                results.Add(new BaseEntityWrapper(trueBaseEntity));
            }
            else
            {
                try
                {
                    _octaneService = OctaneServices.GetInstance();
                    await _octaneService.Connect();

                    EntityListResult<BaseEntity> entities;

                    List<QueryPhrase> filterQuery = new List<QueryPhrase> { new LogicalQueryPhrase("name", "*" + filterString + "*") };

                    if (_fieldEntity.Contains("list_node") && !string.IsNullOrEmpty(logicalName))
                    {
                        entities = await _octaneService.GetAsyncEntitesReferenceListNodes(_fieldEntity, logicalName);                        
                    }
                    else
                    {
                        if (_fieldEntity.Equals("workspace_users"))
                        {
                            filterQuery.Add(new LogicalQueryPhrase("activity_level", 0));
                            entities = await _octaneService.GetAsyncEntitesReferenceFields(_fieldEntity, filterQuery, null, null);
                        }
                        else
                        {
                            if (_fieldEntity.Equals("sprints") || _fieldEntity.Equals("milestones"))
                            {
                                string sortingFields = _fieldEntity.Equals("sprints") ? "-release,start_date,name" : "-release,date,name";
                                string parentsReleaseId = _parentEntity.GetValue("release") != null ? parentsReleaseId = ((BaseEntity)_parentEntity.GetValue("release")).Id.ToString() : "-1";
                                
                                filterQuery.Add(new CrossQueryPhrase("release", new LogicalQueryPhrase("id", parentsReleaseId)));

                                entities = await _octaneService.GetAsyncEntitesReferenceFields(_fieldEntity, filterQuery, null, sortingFields);
                            }
                            else
                            {
                                entities = await _octaneService.GetAsyncEntitesReferenceFields(_fieldEntity, filterQuery, null, null);
                            }
                        }
                    }

                    _referenceFieldContent = entities.data;

                    if (_referenceFieldContent != null)
                    {
                        foreach (BaseEntity be in _referenceFieldContent)
                        {
                            BaseEntityWrapper bew = new BaseEntityWrapper(be);
                            results.Add(bew);

                            var selectedEntities = _parentEntity.GetValue(Name);

                            if (selectedEntities != null && selectedEntities is EntityList<BaseEntity>)
                            {
                                EntityList<BaseEntity> selectedEntitiesList = (EntityList<BaseEntity>)selectedEntities;

                                foreach (BaseEntity sbe in selectedEntitiesList.data)
                                {
                                    if (bew.Equals(new BaseEntityWrapper(sbe)))
                                    {
                                        bew.IsSelected = true;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    return null;
                }
            }

            // List nodes are not server side filterable, so we have to do it client side
            if (_fieldEntity != null && _fieldEntity.Contains("list_node") && !filterString.Equals(""))
            {
                return results.Where(e => e.BaseEntity.Name.ToLowerInvariant().Contains(filterString)).ToList();
            }

            return results;
        }

        private List<BaseEntity> getFieldsBasedOnCurrentRelease(List<BaseEntity> data)
        {
            List<BaseEntity> referenceFieldContent = new List<BaseEntity>();
            BaseEntity parentsRelease = (BaseEntity)_parentEntity.GetValue("release");

            foreach (BaseEntity be in data)
            {
                BaseEntity sprintRelease = (BaseEntity)be.GetValue("release");
                if (parentsRelease != null && sprintRelease.Name.Equals(parentsRelease.Name))
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

        public bool IsMultiple { get; set; }

        public bool IsMoreThanOneTarget { get; set; }

        private String _tempDecimalStrValue { get; set; }

        private object _prevContent { get; set; }

        public object Content
        {
            get
            {
                if(_tempDecimalStrValue != null)
                {
                    return _tempDecimalStrValue;
                }

                if (_customContentFunc != null)
                    return _customContentFunc(_parentEntity);

                var formattedValue = FieldsMetadataService.GetFormattedValue(_parentEntity, Name, Metadata);
                if (formattedValue != null)
                    return formattedValue;

                object value = _parentEntity.GetValue(Name);
                _prevContent = value;

                switch (value)
                {
                    case null:
                        return _emptyPlaceholder;
                    case BaseEntity entity:
                        return new BaseEntityWrapper(entity);
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
                        if (((BaseEntity)value).Name.Equals("False"))
                        {
                            _parentEntity.SetValue(Name, false);
                        }
                        else
                        {
                            _parentEntity.SetValue(Name, true);
                        }
                        IsChanged = true;
                        break;
                    case "integer":
                        try
                        {
                            _tempDecimalStrValue = null;
                            String strValue = value.ToString();
                            if (int.TryParse(strValue, out int intValue))
                            {
                                _parentEntity.SetValue(Name, intValue);
                            }
                            else if (isIntermediaryIntegerValue(strValue))
                            {
                                _tempDecimalStrValue = strValue;
                            }
                            else
                            {
                                _parentEntity.SetValue(Name, _prevContent);
                            }
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
                    case "float":
                        _tempDecimalStrValue = null;
                        String srtValue = value.ToString();

                        if (srtValue != null && srtValue.Trim().Length == 0)
                        {
                            _parentEntity.SetValue(Name, null);
                        }
                        else if (decimal.TryParse(srtValue, out decimal decimalValue))
                        {
                            if (srtValue.EndsWith("."))
                            {
                                _parentEntity.SetValue(Name, decimalValue + ".0");
                            } 
                            else
                            {
                                _parentEntity.SetValue(Name, decimalValue);
                            }
                        }
                        else if (isIntermediaryDecimalValue(srtValue))
                        {
                            _tempDecimalStrValue = srtValue;
                        }
                        else
                        {
                            _parentEntity.SetValue(Name, _prevContent);
                        }
                        IsChanged = true;
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
                        if (!IsMultiple)
                        {
                            _parentEntity.SetValue(Name, (BaseEntity)value);
                        }
                        else
                        {
                            _parentEntity.SetValue(Name, value);
                        }
                        IsChanged = true;
                        break;

                }
                // notify save button 
                OnValueChanged(EventArgs.Empty);
                NotifyPropertyChanged("Content");
            }
        }

        private static bool isIntermediaryIntegerValue(String value)
        {
            if (value == null)
            {
                return false;
            }

            var match = Regex.Match(value, "^[-+]?\\d*$");
            if (match.Success && match.Value.Length == value.Length)
            {
                return true;
            }

            if (value.Trim().Equals("-"))
            {
                return true;
            }

            return false;
        }

        private static bool isIntermediaryDecimalValue(String value)
        {
            if (value == null)
            {
                return false;
            }

            var match = Regex.Match(value, "^-{0,1}\\d*\\.0*$");
            if (match.Success && match.Value.Length == value.Length)
            {
                return true;
            }
            if (value.Trim().Equals("-"))
            {
                return true;
            }

            if (value.EndsWith("."))
            {
                String partialNumber = value.TrimEnd(new char[] { '.' });
                return int.TryParse(partialNumber, out _);
            }

            return false;
        }

        public EntityList<BaseEntity> GetSelectedEntities()
        {
            object value = _parentEntity.GetValue(Name);
            if (value == null)
            {
                return new EntityList<BaseEntity>();
            }
            else
            {
                return (EntityList<BaseEntity>)value;
            }
        }

        public ICommand MakeValueNull { get; }

        private void SetMakeValueNull(object param)
        {
            // This is used to clear the reference field items ( 'x' button on the right side of the EntityComboBox )
            if (Metadata.FieldType.Equals("reference"))
            {
                if (!IsMultiple)
                {
                    Content = null;
                }
                else
                {
                    //clear the parent entity's selected values
                    EntityList<BaseEntity> entities = _parentEntity.GetValue(Name) as EntityList<BaseEntity>;
                    entities.data.Clear();
                    //clear the selected fields also
                    GetSelectedEntities().data.Clear();
                    Content = entities;
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
            return string.Join(",", entityNames);
        }
    }



    public class BaseEntityWrapper
    {
        public BaseEntity BaseEntity { get; }

        public bool IsSelected { get; set; }

        public EntityList<BaseEntity> BaseEntityList { get; }

        public BaseEntityWrapper(BaseEntity entity)
        {
            BaseEntity = entity;
        }

        public BaseEntityWrapper(EntityList<BaseEntity> entityList)
        {
            BaseEntityList = entityList;
        }

        public override string ToString()
        {
            return BaseEntity.Name;
        }

        public override int GetHashCode()
        {
            return BaseEntity.Name.GetHashCode();
        }


        public override bool Equals(object obj)
        {
            if (obj is BaseEntityWrapper)
            {
                return this.BaseEntity.Name.Equals(((BaseEntityWrapper)obj).BaseEntity.Name);
            }
            else
            {
                return false;
            }

        }
    }
}
