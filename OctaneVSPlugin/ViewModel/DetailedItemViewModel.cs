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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;

namespace MicroFocus.Adm.Octane.VisualStudio.ViewModel
{
    /// <summary>
    /// Detailed view model for an entity
    /// </summary>
    public class DetailedItemViewModel : BaseItemViewModel, INotifyPropertyChanged
    {
        private readonly OctaneServices _octaneService;

        private ObservableCollection<CommentViewModel> _commentViewModels;
        private ObservableCollection<FieldViewModel> _visibleFields;
        private readonly List<FieldViewModel> _allEntityFields;
        private ObservableCollection<FieldViewModel> _displayedEntityFields;

        public DetailedItemViewModel(BaseEntity entity, MyWorkMetadata myWorkMetadata)
            : base(entity, myWorkMetadata)
        {
            RefreshCommand = new DelegatedCommand(Refresh);
            ToggleCommentSectionCommand = new DelegatedCommand(SwitchCommentSectionVisibility);
            CheckboxChangeCommand = new DelegatedCommand(CheckboxChange);
            ResetFieldsCustomizationCommand = new DelegatedCommand(ResetFieldsCustomization);

            _commentViewModels = new ObservableCollection<CommentViewModel>();
            _visibleFields = new ObservableCollection<FieldViewModel>();
            _allEntityFields = new List<FieldViewModel>();
            _displayedEntityFields = new ObservableCollection<FieldViewModel>();

            Mode = DetailsWindowMode.LoadingItem;

            if (EntityTypesSupportingComments.Contains(Utility.GetConcreteEntityType(entity)))
                EntitySupportsComments = true;

            _octaneService = new OctaneServices(
                OctaneConfiguration.Url,
                OctaneConfiguration.SharedSpaceId,
                OctaneConfiguration.WorkSpaceId,
                OctaneConfiguration.Username,
                OctaneConfiguration.Password);
        }

        private static readonly Dictionary<string, List<FieldMetadata>> CurrentFieldsCache = new Dictionary<string, List<FieldMetadata>>();

        public async System.Threading.Tasks.Task Initialize()
        {
            try
            {
                await _octaneService.Connect();

                var entityType = Utility.GetConcreteEntityType(Entity);
                List<FieldMetadata> fields;
                if (!CurrentFieldsCache.TryGetValue(entityType, out fields))
                {
                    var fieldsMetadata = await _octaneService.GetFieldsMetadata(entityType);
                    fields = fieldsMetadata.Where(fm => fm.visible_in_ui).ToList();
                    CurrentFieldsCache[entityType] = fields;
                }

                var updatedFields = fields.Select(fm => fm.name).ToList();
                // TODO - investigate why not all entities receive the subtype field by default
                if (MyWorkMetadata.IsAggregateEntity(Entity.GetType()))
                {
                    updatedFields.Add(CommonFields.SUB_TYPE);
                }

                Entity = await _octaneService.FindEntity(Entity, updatedFields);

                _allEntityFields.Clear();
                _visibleFields.Clear();
                _displayedEntityFields.Clear();

                var visibleFieldsHashSet = FieldsCache.Instance.GetVisibleFieldsForEntity(entityType);
                // we want to filter out description because it will be shown separately
                // and subtype because it is only used internally
                foreach (var field in fields.Where(f => f.name != CommonFields.DESCRIPTION && f.name != CommonFields.SUB_TYPE))
                {
                    var fieldViewModel = new FieldViewModel(Entity, field.name, field.label, visibleFieldsHashSet.Contains(field.name));

                    _allEntityFields.Add(fieldViewModel);
                    _displayedEntityFields.Add(fieldViewModel);
                }

                UpdateVisibleFields();

                if (EntitySupportsComments)
                    await RetrieveComments();

                Mode = DetailsWindowMode.ItemLoaded;
            }
            catch (Exception ex)
            {
                Mode = DetailsWindowMode.FailedToLoad;
                ErrorMessage = ex.Message;
            }
            NotifyPropertyChanged();
        }

        public void RefreshFields()
        {
            var entityType = Utility.GetConcreteEntityType(Entity);

            var persistedVisibleFields = FieldsCache.Instance.GetVisibleFieldsForEntity(entityType);

            var newDisplayedEntityFields = new List<FieldViewModel>();
            foreach (var field in _allEntityFields)
            {
                field.IsSelected = persistedVisibleFields.Contains(field.Name);
                newDisplayedEntityFields.Add(field);
            }

            _displayedEntityFields = new ObservableCollection<FieldViewModel>(newDisplayedEntityFields);

            UpdateVisibleFields();

            NotifyPropertyChanged("DisplayedEntityFields");
        }

        public IEnumerable<FieldViewModel> DisplayedEntityFields
        {
            get { return _displayedEntityFields; }
        }

        public IEnumerable<FieldViewModel> VisibleFields
        {
            get { return _visibleFields; }
        }

        private string _filter;

        public string Filter
        {
            get { return _filter; }
            set
            {
                _filter = value.ToLowerInvariant();

                _displayedEntityFields.Clear();
                foreach (var field in _allEntityFields.Where(f => f.Label.ToLowerInvariant().Contains(_filter)))
                {
                    _displayedEntityFields.Add(field);
                }
                NotifyPropertyChanged("DisplayedEntityFields");
            }
        }

        public ICommand CheckboxChangeCommand { get; }

        private void CheckboxChange(object param)
        {
            var entityType = Utility.GetConcreteEntityType(Entity);

            FieldsCache.Instance.UpdateVisibleFieldsForEntity(entityType, _allEntityFields);
            UpdateVisibleFields();
        }

        private void UpdateVisibleFields()
        {
            var newVisibleFields = new List<FieldViewModel>();
            foreach (var field in _allEntityFields.Where(f => f.IsSelected))
            {
                newVisibleFields.Add(field);
            }

            _visibleFields = new ObservableCollection<FieldViewModel>(newVisibleFields);

            OnlyDefaultFieldsAreShown = FieldsCache.Instance.AreSameFieldsAsDefaultFields(Utility.GetConcreteEntityType(Entity), newVisibleFields);

            NotifyPropertyChanged("OnlyDefaultFieldsAreShown");
            NotifyPropertyChanged("VisibleFields");
        }

        public ICommand ResetFieldsCustomizationCommand { get; }

        private void ResetFieldsCustomization(object param)
        {
            var entityType = Utility.GetConcreteEntityType(Entity);
            FieldsCache.Instance.ResetVisibleFieldsForEntity(entityType);

            RefreshFields();
        }

        public bool OnlyDefaultFieldsAreShown { get; private set; }

        public string ErrorMessage { get; private set; }

        private async System.Threading.Tasks.Task RetrieveComments()
        {
            try
            {
                var viewModels = new List<CommentViewModel>();
                var commentEntities = await _octaneService.GetAttachedCommentsToEntity(Entity);
                foreach (var comment in commentEntities)
                {
                    viewModels.Add(new CommentViewModel(comment, MyWorkMetadata));
                }

                _commentViewModels = new ObservableCollection<CommentViewModel>(viewModels.OrderByDescending(c => c.CreationTime));
            }
            catch (Exception)
            {
                _commentViewModels.Clear();
            }
        }

        public override string Description
        {
            get { return Mode != DetailsWindowMode.LoadingItem ? Entity.GetStringValue(CommonFields.DESCRIPTION) ?? string.Empty : string.Empty; }
        }

        public override string IconText
        {
            get { return Mode != DetailsWindowMode.LoadingItem ? MyWorkMetadata.GetIconText(Entity) : null; }
        }

        public override Color IconBackgroundColor
        {
            get { return Mode != DetailsWindowMode.LoadingItem ? MyWorkMetadata.GetIconColor(Entity) : new Color(); }
        }

        public IEnumerable<CommentViewModel> Comments
        {
            get { return _commentViewModels; }
        }

        public DetailsWindowMode Mode { get; private set; }

        public bool EntitySupportsComments { get; }

        private static readonly HashSet<string> EntityTypesSupportingComments = new HashSet<string>
        {
            // work item
            WorkItem.SUBTYPE_DEFECT,
            WorkItem.SUBTYPE_STORY,
            WorkItem.SUBTYPE_QUALITY_STORY,

            // test
            TestGherkin.SUBTYPE_GHERKIN_TEST,
            Test.SUBTYPE_MANUAL_TEST,

            // run
            RunManual.SUBTYPE_RUN_MANUAL,
            RunSuite.SUBTYPE_RUN_SUITE,

            // requirement
            Requirement.SUBTYPE_DOCUMENT
        };

        public bool CommentSectionVisibility { get; set; }

        public ICommand RefreshCommand { get; }

        private void Refresh(object param)
        {
            Mode = DetailsWindowMode.LoadingItem;
            NotifyPropertyChanged();
            Initialize();
        }

        public ICommand ToggleCommentSectionCommand { get; }

        private void SwitchCommentSectionVisibility(object param)
        {
            CommentSectionVisibility = !CommentSectionVisibility;
            NotifyPropertyChanged("CommentSectionVisibility");
            NotifyPropertyChanged("ShowCommentTooltip");
        }

        public string ShowCommentTooltip
        {
            get { return CommentSectionVisibility ? "Hide comments" : "Show comments"; }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private void NotifyPropertyChanged(string propName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
    }
}
