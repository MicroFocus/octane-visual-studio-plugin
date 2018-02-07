using MicroFocus.Adm.Octane.Api.Core.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;

namespace Hpe.Nga.Octane.VisualStudio
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
        private readonly Type[] EntitiesWithoutSubtype = new[] { typeof(Task) };

        public MyWorkMetadata()
        {
            entitiesFetchInfo = new Dictionary<Type, Dictionary<string, ItemSubTypeInfo>>();
            fieldsByEntityType = new Dictionary<Type, List<string>>();
            subTypesByEntityType = new Dictionary<Type, HashSet<string>>();

            AddSubType<WorkItem>(WorkItem.SUBTYPE_DEFECT,
                    "defect",
                    "D", Color.FromRgb(190, 102, 92),
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
                "US", Color.FromRgb(218, 199, 120),
                FieldAtSubTitle(CommonFields.RELEASE, "Release", "No release"),
                FieldAtTop(CommonFields.PHASE, "Phase"),
                FieldAtTop(CommonFields.STORY_POINTS, "SP"),
                FieldAtTop(CommonFields.OWNER, "Owner"),
                FieldAtTop(CommonFields.AUTHOR, "Author"),
                FieldAtBottom(CommonFields.INVESTED_HOURS, "Invested Hours"),
                FieldAtBottom(CommonFields.REMAINING_HOURS, "Remaining Hours"),
                FieldAtBottom(CommonFields.ESTIMATED_HOURS, "Estimated Hours")
                );

            AddSubType<WorkItem>(WorkItem.SUBTYPE_QUALITY_STORY,
                "quality story",
                "QS", Color.FromRgb(95, 112, 118),
                FieldAtSubTitle(CommonFields.RELEASE, "Release", "No release"),
                FieldAtTop(CommonFields.PHASE, "Phase"),
                FieldAtTop(CommonFields.STORY_POINTS, "SP"),
                FieldAtTop(CommonFields.OWNER, "Owner"),
                FieldAtTop(CommonFields.AUTHOR, "Author"),
                FieldAtBottom(CommonFields.INVESTED_HOURS, "Invested Hours"),
                FieldAtBottom(CommonFields.REMAINING_HOURS, "Remaining Hours"),
                FieldAtBottom(CommonFields.ESTIMATED_HOURS, "Estimated Hours")
                );

            AddSubType<Test>("test_manual",
                COMMIT_MESSAGE_NOT_APPLICABLE,
                "MT", Color.FromRgb(96, 121, 141),
                FieldAtSubTitle("test_type", "Test Type"),
                FieldAtTop(CommonFields.PHASE, "Phase"),
                FieldAtTop(CommonFields.OWNER, "Owner"),
                FieldAtTop(CommonFields.AUTHOR, "Author"),
                FieldAtBottom(CommonFields.STEPS_NUM, "Steps"),
                FieldAtBottom(CommonFields.AUTOMATION_STATUS, "Automation status")
                );

            AddSubType<Test>("gherkin_test",
                COMMIT_MESSAGE_NOT_APPLICABLE,
                "GT", Color.FromRgb(120, 196, 192),
                FieldAtSubTitle("test_type", "Test Type"),
                FieldAtTop(CommonFields.PHASE, "Phase"),
                FieldAtTop(CommonFields.OWNER, "Owner"),
                FieldAtTop(CommonFields.AUTHOR, "Author"),
                FieldAtBottom(CommonFields.AUTOMATION_STATUS, "Automation status")
                );

            AddSubType<Run>("run_suite",
                COMMIT_MESSAGE_NOT_APPLICABLE,
                "SR", Color.FromRgb(133, 169, 188),
                FieldAtSubTitle(CommonFields.ENVIROMENT, "Environment", "[No environment]"),
                FieldAtTop(CommonFields.TEST_RUN_NATIVE_STATUS, "Status"),
                FieldAtBottom(CommonFields.STARTED, "Started")
                );

            AddSubType<Run>("run_manual",
                COMMIT_MESSAGE_NOT_APPLICABLE,
                "MR", Color.FromRgb(133, 169, 188),
                FieldAtSubTitle(CommonFields.ENVIROMENT, "Environment", "[No environment]"),
                FieldAtTop(CommonFields.TEST_RUN_NATIVE_STATUS, "Status"),
                FieldAtBottom(CommonFields.STARTED, "Started")
                );

            AddSubType<Requirement>(Requirement.SUBTYPE_DOCUMENT,
                COMMIT_MESSAGE_NOT_APPLICABLE,
                "R", Color.FromRgb(215, 194, 56),
                FieldAtSubTitle(CommonFields.PHASE, "Phase"),
                FieldAtTop(CommonFields.AUTHOR, "Author")
                );

            AddSubType<Task>(SIMPLE_ENTITY_SUBTYPE_PLACEHOLDER,
                "task",
                "T",
                Color.FromRgb(137, 204, 174),
                FieldAtSubTitle(Task.STORY_FIELD, "Story"),
                FieldAtTop(Task.OWNER_FIELD, "Owner"),
                FieldAtTop(Task.PHASE_FIELD, "Phase"),
                FieldAtTop(Task.AUTHOR_FIELD, "Author"),
                FieldAtBottom(Task.INVESTED_HOURS_FIELD, "Invested Hours"),
                FieldAtBottom(Task.REMAINING_HOURS_FIELD, "Remaining Hours"),
                FieldAtBottom(Task.ESTIMATED_HOURS_FIELD, "Estimated Hours")
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

        private FieldInfo FieldAtTop(string field, string title, string emptyPlaceholder = "")
        {
            return new FieldInfo(field, title, emptyPlaceholder, FieldPosition.Top);
        }

        private FieldInfo FieldAtSubTitle(string field, string title, string emptyPlaceholder = "")
        {
            return new FieldInfo(field, title, emptyPlaceholder, FieldPosition.SubTitle);
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
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        private bool IsAggregateEntity(Type entityType)
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
