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
using System.Windows.Media;

namespace MicroFocus.Adm.Octane.VisualStudio
{
    /// <summary>
    /// This is one-stop-shop for entity information to fetch and display.
    /// </summary>
    public class MyWorkMetadata
    {
        /// <summary>
        /// One stop shop for entities fields, icons and any data required for fetch and display them.
        /// </summary>
        private Dictionary<Type, Dictionary<string, ItemSubTypeInfo>> entitiesFetchInfo;

        // Cache subtypes
        private Dictionary<Type, HashSet<string>> subTypesByEntityType;

        // Cache fields
        private Dictionary<Type, List<string>> fieldsByEntityType;

        /// <summary>
        /// Placeholder value for entities without subtype.
        /// </summary>
        private const string SIMPLE_ENTITY_SUBTYPE_PLACEHOLDER = "SimpleEntity";

        /// <summary>
        /// Placeholder value for entities that should not support copy of commit message.
        /// </summary>
        private const string COMMIT_MESSAGE_NOT_APPLICABLE = "";

        /// <summary>
        /// Array of entity types that do not have sub-types.
        /// 
        /// Most of the entites are aggregated entities with sub-types, for the exceptions listed
        /// here the Subtype field is not fetched.
        /// </summary>
        private static readonly Type[] EntitiesWithoutSubtype = new[] { typeof(Task), typeof(Comment) };

        public MyWorkMetadata()
        {
            entitiesFetchInfo = new Dictionary<Type, Dictionary<string, ItemSubTypeInfo>>();
            fieldsByEntityType = new Dictionary<Type, List<string>>();
            subTypesByEntityType = new Dictionary<Type, HashSet<string>>();

            AddSubType<WorkItem>(WorkItem.SUBTYPE_DEFECT,
                "defect",
                EntityNames.GetInitials(WorkItem.SUBTYPE_DEFECT), Color.FromRgb(190, 102, 92),
                FieldAtSubTitle(CommonFields.ENVIROMENT, "Environment", "No environment"),
                FieldAtTop(CommonFields.OWNER, "Owner"),
                FieldAtTop(CommonFields.DETECTED_BY, "Detected By"),
                FieldAtTop(CommonFields.STORY_POINTS, "SP"),
                FieldAtTop(CommonFields.SEVERITY, "Severity"),
                FieldAtBottom(CommonFields.INVESTED_HOURS, "Invested Hours"),
                FieldAtBottom(CommonFields.REMAINING_HOURS, "Remaining Hours"),
                FieldAtBottom(CommonFields.ESTIMATED_HOURS, "Estimated Hours")
                );

            AddSubType<WorkItem>(WorkItem.SUBTYPE_STORY,
                "user story",
                EntityNames.GetInitials(WorkItem.SUBTYPE_STORY), Color.FromRgb(218, 199, 120),
                FieldAtSubTitle(CommonFields.RELEASE, "Release", "No release"),
                FieldAtTop(CommonFields.PHASE, "Phase"),
                FieldAtTop(CommonFields.STORY_POINTS, "SP"),
                FieldAtTop(CommonFields.OWNER, "Owner"),
                FieldAtTop(CommonFields.AUTHOR, "Author", string.Empty, Utility.GetAuthorFullName),
                FieldAtBottom(CommonFields.INVESTED_HOURS, "Invested Hours"),
                FieldAtBottom(CommonFields.REMAINING_HOURS, "Remaining Hours"),
                FieldAtBottom(CommonFields.ESTIMATED_HOURS, "Estimated Hours")
                );

            AddSubType<WorkItem>(WorkItem.SUBTYPE_QUALITY_STORY,
                "quality story",
                EntityNames.GetInitials(WorkItem.SUBTYPE_QUALITY_STORY), Color.FromRgb(95, 112, 118),
                FieldAtSubTitle(CommonFields.RELEASE, "Release", "No release"),
                FieldAtTop(CommonFields.PHASE, "Phase"),
                FieldAtTop(CommonFields.STORY_POINTS, "SP"),
                FieldAtTop(CommonFields.OWNER, "Owner"),
                FieldAtTop(CommonFields.AUTHOR, "Author", string.Empty, Utility.GetAuthorFullName),
                FieldAtBottom(CommonFields.INVESTED_HOURS, "Invested Hours"),
                FieldAtBottom(CommonFields.REMAINING_HOURS, "Remaining Hours"),
                FieldAtBottom(CommonFields.ESTIMATED_HOURS, "Estimated Hours")
                );

            AddSubType<Test>(Test.SUBTYPE_MANUAL_TEST,
                COMMIT_MESSAGE_NOT_APPLICABLE,
                EntityNames.GetInitials(Test.SUBTYPE_MANUAL_TEST), Color.FromRgb(96, 121, 141),
                FieldAtSubTitle("test_type", "Test Type"),
                FieldAtTop(CommonFields.PHASE, "Phase"),
                FieldAtTop(CommonFields.OWNER, "Owner"),
                FieldAtTop(CommonFields.AUTHOR, "Author", string.Empty, Utility.GetAuthorFullName),
                FieldAtBottom(CommonFields.STEPS_NUM, "Steps"),
                FieldAtBottom(CommonFields.AUTOMATION_STATUS, "Automation status")
                );

            AddSubType<Test>(TestGherkin.SUBTYPE_GHERKIN_TEST,
                COMMIT_MESSAGE_NOT_APPLICABLE,
                EntityNames.GetInitials(TestGherkin.SUBTYPE_GHERKIN_TEST), Color.FromRgb(120, 196, 192),
                FieldAtSubTitle("test_type", "Test Type"),
                FieldAtTop(CommonFields.PHASE, "Phase"),
                FieldAtTop(CommonFields.OWNER, "Owner"),
                FieldAtTop(CommonFields.AUTHOR, "Author", string.Empty, Utility.GetAuthorFullName),
                FieldAtBottom(CommonFields.AUTOMATION_STATUS, "Automation status")
                );

            AddSubType<Run>(RunSuite.SUBTYPE_RUN_SUITE,
                COMMIT_MESSAGE_NOT_APPLICABLE,
                EntityNames.GetInitials(RunSuite.SUBTYPE_RUN_SUITE), Color.FromRgb(133, 169, 188),
                FieldAtSubTitle(CommonFields.ENVIROMENT, "Environment", "[No environment]"),
                FieldAtTop(CommonFields.TEST_RUN_NATIVE_STATUS, "Status"),
                FieldAtBottom(CommonFields.STARTED, "Started")
                );

            AddSubType<Run>(RunManual.SUBTYPE_RUN_MANUAL,
                COMMIT_MESSAGE_NOT_APPLICABLE,
                EntityNames.GetInitials(RunManual.SUBTYPE_RUN_MANUAL), Color.FromRgb(133, 169, 188),
                FieldAtSubTitle(CommonFields.ENVIROMENT, "Environment", "[No environment]"),
                FieldAtTop(CommonFields.TEST_RUN_NATIVE_STATUS, "Status"),
                FieldAtBottom(CommonFields.STARTED, "Started")
                );

            AddSubType<Requirement>(Requirement.SUBTYPE_DOCUMENT,
                COMMIT_MESSAGE_NOT_APPLICABLE,
                EntityNames.GetInitials(Requirement.SUBTYPE_DOCUMENT), Color.FromRgb(215, 194, 56),
                FieldAtSubTitle(CommonFields.PHASE, "Phase"),
                FieldAtTop(CommonFields.AUTHOR, "Author", string.Empty, Utility.GetAuthorFullName)
                );

            AddSubType<Task>(SIMPLE_ENTITY_SUBTYPE_PLACEHOLDER,
                "task",
                EntityNames.GetInitials(Task.TYPE_TASK),
                Color.FromRgb(137, 204, 174),
                FieldAtSubTitle(Task.STORY_FIELD, string.Empty, string.Empty, entity =>
                {
                    var parentEntity = entity.GetValue("story") as BaseEntity;
                    if (parentEntity == null)
                        return string.Empty;

                    var sb = new StringBuilder("Task of ")
                        .Append(EntityNames.GetDisplayName(Utility.GetConcreteEntityType(parentEntity)).ToLower())
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
                COMMIT_MESSAGE_NOT_APPLICABLE,
                EntityNames.GetInitials("comment"), Color.FromRgb(234, 179, 124),
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

        internal string GetIconText(BaseEntity entity)
        {
            return GetEntitySubTypeInfo(entity).IconInfo.ShortLabel;
        }

        internal Color GetIconColor(BaseEntity entity)
        {
            return GetEntitySubTypeInfo(entity).IconInfo.LabelColor;
        }

        internal bool IsSupportCopyCommitMessage(BaseEntity entity)
        {
            return COMMIT_MESSAGE_NOT_APPLICABLE != GetEntitySubTypeInfo(entity).CommitMessageTypeName;
        }

        internal string GetCommitMessageTypeName(BaseEntity entity)
        {
            return GetEntitySubTypeInfo(entity).CommitMessageTypeName;
        }

        private ItemSubTypeInfo GetEntitySubTypeInfo(BaseEntity entity)
        {
            string subType = GetEntitySubType(entity);
            return entitiesFetchInfo[entity.GetType()][subType];
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
                from field in entitiesFetchInfo[entityType][subType].Fields
                where field.Position == position
                select field;

            return fieldByTypeQuery;
        }

        private string GetEntitySubType(BaseEntity entity)
        {
            string subType;
            if (IsAggregateEntity(entity.GetType()))
            {
                subType = (string)entity.GetValue(CommonFields.SUB_TYPE);
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

        /// <summary>
        /// Get work item subtypes we are interested in.
        /// </summary>
        public ISet<string> SubtypesForType<TEntity>()
        {
            HashSet<string> subtypes;

            if (!subTypesByEntityType.TryGetValue(typeof(TEntity), out subtypes))
            {
                subtypes = new HashSet<string>(entitiesFetchInfo[typeof(TEntity)].Keys);
                subTypesByEntityType.Add(typeof(TEntity), subtypes);
            }
            return subtypes;
        }

        public List<string> FieldsForType<TEntity>()
        {
            List<string> fields;

            if (!fieldsByEntityType.TryGetValue(typeof(TEntity), out fields))
            {
                // Create a flat list of fields
                fields = entitiesFetchInfo[typeof(TEntity)]
                    .Values
                    .SelectMany(x => x.Fields)
                    .Select(x => x.Name)
                    .ToList();

                // Add common fields
                fields.Add(CommonFields.DESCRIPTION);
                fields.Add(CommonFields.NAME);
                fields.Add(CommonFields.LAST_MODIFIED);

                // Some entities are listed as not having subtype field
                if (IsAggregateEntity(typeof(TEntity)))
                {
                    fields.Add(CommonFields.SUB_TYPE);
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

        private void AddSubType<TEntityType>(string subtype,
            string commitMessageTypeName,
            string shortLabel, Color labelColor,
            params FieldInfo[] fields)
        {
            Dictionary<string, ItemSubTypeInfo> subTypeFields = GetSubtypesForEntityType<TEntityType>();
            subTypeFields.Add(subtype, new ItemSubTypeInfo(commitMessageTypeName, fields, new ItemIconInfo(shortLabel, labelColor)));
        }

        private Dictionary<string, ItemSubTypeInfo> GetSubtypesForEntityType<TEntityType>()
        {
            Dictionary<string, ItemSubTypeInfo> subTypeFields;

            if (!entitiesFetchInfo.TryGetValue(typeof(TEntityType), out subTypeFields))
            {
                subTypeFields = new Dictionary<string, ItemSubTypeInfo>();
                entitiesFetchInfo.Add(typeof(TEntityType), subTypeFields);
            }

            return subTypeFields;
        }

        private class ItemSubTypeInfo
        {
            public FieldInfo[] Fields { get; set; }
            public ItemIconInfo IconInfo { get; set; }
            public string CommitMessageTypeName { get; set; }

            public ItemSubTypeInfo(string commitMessageTypeName, FieldInfo[] fields, ItemIconInfo iconInfo)
            {
                CommitMessageTypeName = commitMessageTypeName;
                Fields = fields;
                IconInfo = iconInfo;
            }
        }
    }
}
