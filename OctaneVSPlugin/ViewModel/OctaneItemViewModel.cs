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
using System.Linq;
using System.Windows;
using Task = MicroFocus.Adm.Octane.Api.Core.Entities.Task;

namespace MicroFocus.Adm.Octane.VisualStudio.ViewModel
{
    /// <summary>
    /// View model for an entity directly related to the current user
    /// </summary>
    public class OctaneItemViewModel : BaseItemViewModel
    {
        private bool _isActiveItem;

        /// <summary>
        /// Constructor
        /// </summary>
        public OctaneItemViewModel(BaseEntity entity)
            : base(entity)
        {
            SubTitleField = new FieldViewModel(Entity, MyWorkMetadata.Instance.GetSubTitleFieldInfo(entity));

            var topFields = new List<FieldViewModel>();
            foreach (FieldInfo fieldInfo in MyWorkMetadata.Instance.GetTopFieldsInfo(entity))
            {
                topFields.Add(new FieldViewModel(Entity, fieldInfo));
            }
            TopFields = FieldsWithSeparators(topFields).ToList();

            var bottomFields = new List<FieldViewModel>();
            foreach (FieldInfo fieldInfo in MyWorkMetadata.Instance.GetBottomFieldsInfo(entity))
            {
                bottomFields.Add(new FieldViewModel(Entity, fieldInfo));
            }
            BottomFields = FieldsWithSeparators(bottomFields).ToList();
        }

        public virtual bool VisibleID { get { return true; } }

        #region ActiveItem

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
        /// Reference to current active item
        /// </summary>
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

        #endregion

        #region CommitMessage

        /// <summary>
        /// Commit message for item
        /// </summary>
        public string CommitMessage
        {
            get
            {
                if (!IsSupportCopyCommitMessage)
                    return string.Empty;

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

        /// <summary>
        /// Flag whether the current item supports a commit message
        /// </summary>
        public bool IsSupportCopyCommitMessage
        {
            get { return EntityTypeInformation.IsCopyCommitMessageSupported; }
        }

        /// <summary>
        /// Validate current item's commit mesasge against Octane's template
        /// </summary>
        public async System.Threading.Tasks.Task ValidateCommitMessage()
        {
            if (!EntityTypeInformation.IsCopyCommitMessageSupported)
                return;

            OctaneServices octaneService = OctaneServices.GetInstance();
           
            var commitPatterns = await octaneService.ValidateCommitMessageAsync(CommitMessage);

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

        #endregion


        #region Fields

        /// <summary>
        /// Field info shown below the title
        /// </summary>
        public FieldViewModel SubTitleField { get; }

        /// <summary>
        /// Field enumeration shown at the top of the view
        /// </summary>
        public IEnumerable<object> TopFields { get; }

        /// <summary>
        /// Field enumeration shown at the bottom of the view
        /// </summary>
        public IEnumerable<object> BottomFields { get; }

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

        #endregion
    }
}
