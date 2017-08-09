using System.Linq;
using System.Collections.Generic;
using System;
using System.Windows.Media;
using Hpe.Nga.Api.Core.Entities;

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

        public MyWorkMetadata()
        {
            entitiesFetchInfo = new Dictionary<Type, Dictionary<string, ItemSubTypeInfo>>();
            fieldsByEntityType = new Dictionary<Type, List<string>>();
            subTypesByEntityType = new Dictionary<Type, HashSet<string>>();

            AddSubType<WorkItem>(WorkItem.SUBTYPE_DEFECT, 
                    "defect",
                    "D", Color.FromRgb(190, 102, 92),
                    FieldAtSubTitle(WorkItemFields.ENVIROMENT, "Environment", "No environment"),
                    FieldAtTop(WorkItemFields.OWNER, "Owner"),
                    FieldAtTop(WorkItemFields.DETECTED_BY, "Detected By"),
                    FieldAtTop(WorkItemFields.STORY_POINTS, "SP"),
                    FieldAtTop(WorkItemFields.SEVERITY, "Severity"),
                    FieldAtBottom(WorkItemFields.INVESTED_HOURS, "Invested Hours"),
                    FieldAtBottom(WorkItemFields.REMAINING_HOURS, "Remaining Hours"),
                    FieldAtBottom(WorkItemFields.ESTIMATED_HOURS, "Estimated Hours")
                    );

            AddSubType<WorkItem>(WorkItem.SUBTYPE_STORY, 
                "user story",
                "US", Color.FromRgb(218, 199, 120),
                FieldAtSubTitle(WorkItemFields.RELEASE, "Release", "No release"),
                FieldAtTop(WorkItemFields.PHASE, "Phase"),
                FieldAtTop(WorkItemFields.STORY_POINTS, "SP"),
                FieldAtTop(WorkItemFields.OWNER, "Owner"),
                FieldAtTop(WorkItemFields.AUTHOR, "Author"),
                FieldAtBottom(WorkItemFields.INVESTED_HOURS, "Invested Hours"),
                FieldAtBottom(WorkItemFields.REMAINING_HOURS, "Remaining Hours"),
                FieldAtBottom(WorkItemFields.ESTIMATED_HOURS, "Estimated Hours")
                );

            AddSubType<WorkItem>(WorkItem.SUBTYPE_QUALITY_STORY, 
                "quality story", 
                "QS", Color.FromRgb(95, 112, 118),
                FieldAtSubTitle(WorkItemFields.RELEASE, "Release", "No release"),
                FieldAtTop(WorkItemFields.PHASE, "Phase"),
                FieldAtTop(WorkItemFields.STORY_POINTS, "SP"),
                FieldAtTop(WorkItemFields.OWNER, "Owner"),
                FieldAtTop(WorkItemFields.AUTHOR, "Author"),
                FieldAtBottom(WorkItemFields.INVESTED_HOURS, "Invested Hours"),
                FieldAtBottom(WorkItemFields.REMAINING_HOURS, "Remaining Hours"),
                FieldAtBottom(WorkItemFields.ESTIMATED_HOURS, "Estimated Hours")
                );

            AddSubType<Test>("test_manual", 
                "manual test", 
                "MT", Color.FromRgb(96, 121, 141),
                FieldAtSubTitle("test_type", "Test Type"),
                FieldAtTop(WorkItemFields.PHASE, "Phase"),
                FieldAtTop(WorkItemFields.OWNER, "Owner"),
                FieldAtTop(WorkItemFields.AUTHOR, "Author"),
                FieldAtBottom(WorkItemFields.STEPS_NUM, "Steps"),
                FieldAtBottom(WorkItemFields.AUTOMATION_STATUS, "Automation status")
                );

            AddSubType<Test>("gherkin_test",
                "gherkin test",
                "GT", Color.FromRgb(120, 196, 192),
                FieldAtSubTitle("test_type", "Test Type"),
                FieldAtTop(WorkItemFields.PHASE, "Phase"),
                FieldAtTop(WorkItemFields.OWNER, "Owner"),
                FieldAtTop(WorkItemFields.AUTHOR, "Author"),
                FieldAtBottom(WorkItemFields.AUTOMATION_STATUS, "Automation status")
                );

            AddSubType<Run>("run_suite",
                "suite run", 
                "SR", Color.FromRgb(133, 169, 188),
                FieldAtSubTitle(WorkItemFields.ENVIROMENT, "Environment", "[No environment]"),
                FieldAtTop(WorkItemFields.TEST_RUN_NATIVE_STATUS, "Status"),
                FieldAtBottom(WorkItemFields.STARTED, "Strated")
                );

            AddSubType<Run>("run_manual",
                "manual run",
                "MR", Color.FromRgb(133, 169, 188),
                FieldAtSubTitle(WorkItemFields.ENVIROMENT, "Environment", "[No environment]"),
                FieldAtTop(WorkItemFields.TEST_RUN_NATIVE_STATUS, "Status"),
                FieldAtBottom(WorkItemFields.STARTED, "Strated")
                );

            AddSubType<Requirement>(Requirement.SUBTYPE_DOCUMENT, 
                "requirement document",
                "R", Color.FromRgb(215, 194, 56),
                FieldAtSubTitle(WorkItemFields.PHASE, "Phase"),
                FieldAtTop(WorkItemFields.AUTHOR, "Author")
                );
        }

        internal IEnumerable<FieldInfo> GetBottomFieldsInfo(BaseEntity entity)
        {
            var subType = (string)entity.GetValue("subtype");
            return GetFieldInfoByType(entity.GetType(), subType, FieldPosition.Bottom);
        }

        internal string GetIconText(BaseEntity entity)
        {
            return entitiesFetchInfo[entity.GetType()][entity.GetStringValue(WorkItemFields.SUB_TYPE)].IconInfo.ShortLabel;
        }

        internal Color GetIconColor(BaseEntity entity)
        {
            return entitiesFetchInfo[entity.GetType()][entity.GetStringValue(WorkItemFields.SUB_TYPE)].IconInfo.LabelColor;
        }

        internal string GetCommitMessageTypeName(BaseEntity entity)
        {
            return entitiesFetchInfo[entity.GetType()][entity.GetStringValue(WorkItemFields.SUB_TYPE)].CommitMessageTypeName;
        }

        internal IEnumerable<FieldInfo> GetTopFieldsInfo(BaseEntity entity)
        {
            var subType = (string)entity.GetValue("subtype");
            return GetFieldInfoByType(entity.GetType(), subType, FieldPosition.Top);
        }

        internal FieldInfo GetSubTitleFieldInfo(BaseEntity entity)
        {
            var subType = (string)entity.GetValue("subtype");
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
                fields.Add(WorkItemFields.DESCRIPTION);
                fields.Add(WorkItemFields.SUB_TYPE);
                fields.Add(WorkItemFields.NAME);

                fieldsByEntityType.Add(typeof(TEntity), fields);
            }

            return fields;
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
