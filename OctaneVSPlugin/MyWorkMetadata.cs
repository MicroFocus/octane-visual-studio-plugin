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
using MicroFocus.Adm.Octane.VisualStudio.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MicroFocus.Adm.Octane.VisualStudio
{
    /// <summary>
    /// This is one-stop-shop for entity information to fetch and display.
    /// </summary>
    public class MyWorkMetadata
    {
        private static MyWorkMetadata _metadata;
        private static readonly object _obj = new object();

        /// <summary>
        /// One stop shop for entities fields required for fetch and display them.
        /// </summary>
        private Dictionary<Type, Dictionary<string, FieldInfo[]>> _entitiesFieldsFetchInfo;

        // Cache fields
        private Dictionary<Type, List<string>> fieldsByEntityType;

        /// <summary>
        /// Placeholder value for entities without subtype.
        /// </summary>
        private const string SIMPLE_ENTITY_SUBTYPE_PLACEHOLDER = "SimpleEntity";

        public static MyWorkMetadata Instance
        {
            get
            {
                if (_metadata == null)
                {
                    lock (_obj)
                    {
                        if (_metadata == null)
                        {
                            _metadata = new MyWorkMetadata();
                        }
                    }
                }

                return _metadata;
            }
        }

        /// <summary>
        /// Array of entity types that do not have sub-types.
        /// 
        /// Most of the entites are aggregated entities with sub-types, for the exceptions listed
        /// here the Subtype field is not fetched.
        /// </summary>
        private static readonly Type[] EntitiesWithoutSubtype = new[] { typeof(Task), typeof(Comment) };

        private MyWorkMetadata()
        {
            _entitiesFieldsFetchInfo = new Dictionary<Type, Dictionary<string, FieldInfo[]>>();
            fieldsByEntityType = new Dictionary<Type, List<string>>();

            AddSubType<WorkItem>(WorkItem.SUBTYPE_DEFECT,
                FieldAtSubTitle(CommonFields.Environment, "Environment", "No environment"),
                FieldAtTop(CommonFields.Owner, "Owner"),
                FieldAtTop(CommonFields.DetectedBy, "Detected By"),
                FieldAtTop(CommonFields.StoryPoints, "SP"),
                FieldAtTop(CommonFields.Severity, "Severity"),
                FieldAtBottom(CommonFields.InvestedHours, "Invested Hours"),
                FieldAtBottom(CommonFields.RemainingHours, "Remaining Hours"),
                FieldAtBottom(CommonFields.EstimatedHours, "Estimated Hours")
                );

            AddSubType<WorkItem>(WorkItem.SUBTYPE_STORY,
                FieldAtSubTitle(CommonFields.Release, "Release", "No release"),
                FieldAtTop(CommonFields.Phase, "Phase"),
                FieldAtTop(CommonFields.StoryPoints, "SP"),
                FieldAtTop(CommonFields.Owner, "Owner"),
                FieldAtTop(CommonFields.Author, "Author", string.Empty, Utility.GetAuthorFullName),
                FieldAtBottom(CommonFields.InvestedHours, "Invested Hours"),
                FieldAtBottom(CommonFields.RemainingHours, "Remaining Hours"),
                FieldAtBottom(CommonFields.EstimatedHours, "Estimated Hours")
                );

            AddSubType<WorkItem>(WorkItem.SUBTYPE_QUALITY_STORY,
                FieldAtSubTitle(CommonFields.Release, "Release", "No release"),
                FieldAtTop(CommonFields.Phase, "Phase"),
                FieldAtTop(CommonFields.StoryPoints, "SP"),
                FieldAtTop(CommonFields.Owner, "Owner"),
                FieldAtTop(CommonFields.Author, "Author", string.Empty, Utility.GetAuthorFullName),
                FieldAtBottom(CommonFields.InvestedHours, "Invested Hours"),
                FieldAtBottom(CommonFields.RemainingHours, "Remaining Hours"),
                FieldAtBottom(CommonFields.EstimatedHours, "Estimated Hours")
                );

            AddSubType<Test>(Test.SUBTYPE_MANUAL_TEST,
                FieldAtSubTitle("test_type", "Test Type"),
                FieldAtTop(CommonFields.Phase, "Phase"),
                FieldAtTop(CommonFields.Owner, "Owner"),
                FieldAtTop(CommonFields.Author, "Author", string.Empty, Utility.GetAuthorFullName),
                FieldAtBottom(CommonFields.StepsNum, "Steps"),
                FieldAtBottom(CommonFields.AutomationStatus, "Automation status")
                );

            AddSubType<Test>(TestGherkin.SUBTYPE_GHERKIN_TEST,
                FieldAtSubTitle("test_type", "Test Type"),
                FieldAtTop(CommonFields.Phase, "Phase"),
                FieldAtTop(CommonFields.Owner, "Owner"),
                FieldAtTop(CommonFields.Author, "Author", string.Empty, Utility.GetAuthorFullName),
                FieldAtBottom(CommonFields.AutomationStatus, "Automation status")
                );

            AddSubType<Run>(RunSuite.SUBTYPE_RUN_SUITE,
                FieldAtSubTitle(CommonFields.Environment, "Environment", "[No environment]"),
                FieldAtTop(CommonFields.TestRunNativeStatus, "Status"),
                FieldAtBottom(CommonFields.Started, "Started")
                );

            AddSubType<Run>(RunManual.SUBTYPE_RUN_MANUAL,
                FieldAtSubTitle(CommonFields.Environment, "Environment", "[No environment]"),
                FieldAtTop(CommonFields.TestRunNativeStatus, "Status"),
                FieldAtBottom(CommonFields.Started, "Started")
                );

            AddSubType<Requirement>(Requirement.SUBTYPE_DOCUMENT,
                FieldAtSubTitle(CommonFields.Phase, "Phase"),
                FieldAtTop(CommonFields.Author, "Author", string.Empty, Utility.GetAuthorFullName)
                );

            AddSubType<Task>(SIMPLE_ENTITY_SUBTYPE_PLACEHOLDER,
                FieldAtSubTitle(Task.STORY_FIELD, string.Empty, string.Empty, entity =>
                {
                    var parentEntity = entity.GetValue("story") as BaseEntity;
                    if (parentEntity == null)
                        return string.Empty;
                    
                    var parentEntityInformation = EntityTypeRegistry.GetEntityTypeInformation(parentEntity);
                    if (parentEntityInformation == null)
                        return string.Empty;

                    var sb = new StringBuilder("Task of ")
                        .Append(parentEntityInformation.DisplayName.ToLower())
                        .Append(" ")
                        .Append(parentEntity.Id.ToString())
                        .Append(": ")
                        .Append(parentEntity.Name);
                    return sb.ToString();

                }),
                FieldAtTop(Task.OWNER_FIELD, "Owner"),
                FieldAtTop(Task.PHASE_FIELD, "Phase"),
                FieldAtTop(Task.AUTHOR_FIELD, "Author", string.Empty, Utility.GetAuthorFullName),
                FieldAtBottom(Task.INVESTED_HOURS_FIELD, "Invested Hours"),
                FieldAtBottom(Task.REMAINING_HOURS_FIELD, "Remaining Hours"),
                FieldAtBottom(Task.ESTIMATED_HOURS_FIELD, "Estimated Hours")
                );

            AddSubType<Comment>(SIMPLE_ENTITY_SUBTYPE_PLACEHOLDER,
                FieldAtSubTitle(Comment.TEXT_FIELD, string.Empty, string.Empty, entity =>
                {
                    return Utility.StripHtml(entity.GetStringValue(Comment.TEXT_FIELD));
                }),
                FieldAtTop(Comment.AUTHOR_FIELD, "Author", string.Empty, Utility.GetAuthorFullName)
                );
        }

        internal IEnumerable<FieldInfo> GetBottomFieldsInfo(BaseEntity entity)
        {
            string subType = GetEntitySubType(entity);
            return GetFieldInfoByType(entity.GetType(), subType, FieldPosition.Bottom);
        }

        internal IEnumerable<FieldInfo> GetTopFieldsInfo(BaseEntity entity)
        {
            string subType = GetEntitySubType(entity);
            return GetFieldInfoByType(entity.GetType(), subType, FieldPosition.Top);
        }

        internal FieldInfo GetSubTitleFieldInfo(BaseEntity entity)
        {
            string subType = GetEntitySubType(entity);
            return GetFieldInfoByType(entity.GetType(), subType, FieldPosition.SubTitle).First();
        }

        private IEnumerable<FieldInfo> GetFieldInfoByType(Type entityType, string subType, FieldPosition position)
        {
            var fieldByTypeQuery =
                from field in _entitiesFieldsFetchInfo[entityType][subType]
                where field.Position == position
                select field;

            return fieldByTypeQuery;
        }

        private string GetEntitySubType(BaseEntity entity)
        {
            string subType;
            if (IsAggregateEntity(entity.GetType()))
            {
                subType = (string)entity.GetValue(CommonFields.SubType);
                Debug.Assert(subType != null, "Entity should have subtype field. If it's simple list it in EntitiesWithoutSubtype array.");
            }
            else
            {
                subType = SIMPLE_ENTITY_SUBTYPE_PLACEHOLDER;
            }

            return subType;
        }

        private FieldInfo FieldAtBottom(string field, string title, string emptyPlaceholder = "")
        {
            return new FieldInfo(field, title, emptyPlaceholder, FieldPosition.Bottom);
        }

        private FieldInfo FieldAtTop(string field, string title, string emptyPlaceholder = "", Func<BaseEntity, object> contentFunc = null)
        {
            return new FieldInfo(field, title, emptyPlaceholder, FieldPosition.Top, contentFunc);
        }

        private FieldInfo FieldAtSubTitle(string field, string title, string emptyPlaceholder = "", Func<BaseEntity, object> contentFunc = null)
        {
            return new FieldInfo(field, title, emptyPlaceholder, FieldPosition.SubTitle, contentFunc);
        }

        public List<string> FieldsForType<TEntity>()
        {
            List<string> fields;

            if (!fieldsByEntityType.TryGetValue(typeof(TEntity), out fields))
            {
                // Create a flat list of fields
                fields = _entitiesFieldsFetchInfo[typeof(TEntity)]
                    .Values
                    .SelectMany(x => x)
                    .Select(x => x.Name)
                    .ToList();

                // Add common fields
                fields.Add(CommonFields.Description);
                fields.Add(CommonFields.Name);
                fields.Add(CommonFields.LastModified);

                // Some entities are listed as not having subtype field
                if (IsAggregateEntity(typeof(TEntity)))
                {
                    fields.Add(CommonFields.SubType);
                }

                fieldsByEntityType.Add(typeof(TEntity), fields);
            }

            return fields;
        }

        /// <summary>
        /// Get indication if the entity is simple or aggregate.
        /// </summary>
        public static bool IsAggregateEntity(Type entityType)
        {
            return -1 == Array.IndexOf<Type>(EntitiesWithoutSubtype, entityType);
        }

        private void AddSubType<TEntityType>(string subtype, params FieldInfo[] fields)
        {
            Dictionary<string, FieldInfo[]> subTypeFields = GetSubtypesForEntityType<TEntityType>();
            subTypeFields.Add(subtype, fields);
        }

        private Dictionary<string, FieldInfo[]> GetSubtypesForEntityType<TEntityType>()
        {
            Dictionary<string, FieldInfo[]> subTypeFields;

            if (!_entitiesFieldsFetchInfo.TryGetValue(typeof(TEntityType), out subTypeFields))
            {
                subTypeFields = new Dictionary<string, FieldInfo[]>();
                _entitiesFieldsFetchInfo.Add(typeof(TEntityType), subTypeFields);
            }

            return subTypeFields;
        }
    }
}
