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
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Task = MicroFocus.Adm.Octane.Api.Core.Entities.Task;

namespace MicroFocus.Adm.Octane.VisualStudio.ViewModel
{
    public class OctaneItemViewModel : BaseItemViewModel
    {
        private bool _isActiveItem;

        private readonly List<FieldViewModel> topFields;
        private readonly List<FieldViewModel> bottomFields;
        private readonly FieldViewModel subTitleField;

        public OctaneItemViewModel(BaseEntity entity)
            : base(entity)
        {
            topFields = new List<FieldViewModel>();
            bottomFields = new List<FieldViewModel>();

            subTitleField = new FieldViewModel(Entity, MyWorkMetadata.Instance.GetSubTitleFieldInfo(entity));

            foreach (FieldInfo fieldInfo in MyWorkMetadata.Instance.GetTopFieldsInfo(entity))
            {
                topFields.Add(new FieldViewModel(Entity, fieldInfo));
            }

            foreach (FieldInfo fieldInfo in MyWorkMetadata.Instance.GetBottomFieldsInfo(entity))
            {
                bottomFields.Add(new FieldViewModel(Entity, fieldInfo));
            }
        }

        public virtual bool VisibleID { get { return true; } }

        public string TypeName
        {
            get { return Entity.TypeName; }
        }

        /// <summary>
        /// Flag specifying whether this entity is the current active work item
        /// </summary>
        public bool IsActiveWorkItem
        {
            get { return _isActiveItem; }
            private set
            {
                _isActiveItem = value;
                NotifyPropertyChanged("IsActiveWorkItem");
            }
        }

        /// <summary>
        /// Validate current item's commit mesasge against Octane's template
        /// </summary>
        public async System.Threading.Tasks.Task ValidateCommitMessage()
        {
            if (!EntityTypeInformation.IsCopyCommitMessageSupported)
                return;

            var octane = new OctaneServices(OctaneConfiguration.Url,
                OctaneConfiguration.SharedSpaceId,
                OctaneConfiguration.WorkSpaceId,
                OctaneConfiguration.Username,
                OctaneConfiguration.Password);
            await octane.Connect();
            var commitPatterns = await octane.ValidateCommitMessageAsync(CommitMessage);

            var type = Utility.GetConcreteEntityType(Entity);
            var expectedId = Entity.Id;
            if (type == Task.TYPE_TASK)
            {
                var parentEntity = Utility.GetTaskParentEntity(Entity);

                type = Utility.GetConcreteEntityType(parentEntity);
                expectedId = parentEntity.Id;
            }

            var result = false;
            switch (type)
            {
                case WorkItem.SUBTYPE_STORY:
                    result = commitPatterns.story.Contains(expectedId);
                    break;
                case WorkItem.SUBTYPE_DEFECT:
                    result = commitPatterns.defect.Contains(expectedId);
                    break;
                case WorkItem.SUBTYPE_QUALITY_STORY:
                    result = commitPatterns.quality_story.Contains(expectedId);
                    break;
            }

            if (result)
                Clipboard.SetText(CommitMessage);
        }

        public static OctaneItemViewModel CurrentActiveItem { get; private set; }

        /// <summary>
        /// Set the given item as the current active item
        /// </summary>
        public static void SetActiveItem(OctaneItemViewModel octaneItem)
        {
            if (octaneItem == null)
                return;

            if (CurrentActiveItem != null)
                CurrentActiveItem.IsActiveWorkItem = false;

            CurrentActiveItem = octaneItem;

            CurrentActiveItem.IsActiveWorkItem = true;
            WorkspaceSessionPersistanceManager.SetActiveEntity(CurrentActiveItem.Entity);
        }

        /// <summary>
        /// Clear the current active item
        /// </summary>
        public static void ClearActiveItem()
        {
            if (CurrentActiveItem != null)
                CurrentActiveItem.IsActiveWorkItem = false;

            CurrentActiveItem = null;
            WorkspaceSessionPersistanceManager.ClearActiveEntity();
        }

        public string CommitMessage
        {
            get
            {
                if (Entity.TypeName == Task.TYPE_TASK)
                {
                    var parentEntity = Utility.GetTaskParentEntity(Entity);
                    var parentEntityTypeInfo = EntityTypeRegistry.GetEntityTypeInformation(parentEntity);

                    return $"{parentEntityTypeInfo.CommitMessage} #{parentEntity.Id}: {EntityTypeInformation.CommitMessage} #{ID}: ";
                }
                else
                {
                    return $"{EntityTypeInformation.CommitMessage} #{ID}: ";
                }
            }
        }

        public bool IsSupportCopyCommitMessage
        {
            get { return EntityTypeInformation.IsCopyCommitMessageSupported; }
        }

        public FieldViewModel SubTitleField
        {
            get { return subTitleField; }
        }

        public IEnumerable<object> TopFields
        {
            get
            {
                return FieldsWithSeparators(topFields);
            }
        }
        public IEnumerable<object> BottomFields
        {
            get
            {
                return FieldsWithSeparators(bottomFields);
            }
        }

        private IEnumerable<object> FieldsWithSeparators(List<FieldViewModel> fields)
        {
            // Handle the case there are no fields so we don't need any seperators.
            if (fields.Count == 0)
            {
                yield break;
            }

            foreach (FieldViewModel field in fields.Take(fields.Count - 1))
            {
                yield return field;
                yield return SeparatorViewModel.Make();
            }

            yield return fields.Last();
        }
    }
}
