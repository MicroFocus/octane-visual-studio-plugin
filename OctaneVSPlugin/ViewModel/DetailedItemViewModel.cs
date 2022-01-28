﻿/*!
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

using MicroFocus.Adm.Octane.Api.Core.Connector.Exceptions;
using MicroFocus.Adm.Octane.Api.Core.Entities;
using MicroFocus.Adm.Octane.Api.Core.Services.Version;
using MicroFocus.Adm.Octane.VisualStudio.Common;
using MicroFocus.Adm.Octane.VisualStudio.View;
using NSoup.Nodes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Task = System.Threading.Tasks.Task;
using TaskEntity = MicroFocus.Adm.Octane.Api.Core.Entities.Task;

namespace MicroFocus.Adm.Octane.VisualStudio.ViewModel
{
    /// <summary>
    /// Detailed view model for an entity
    /// </summary>
    public class DetailedItemViewModel : BaseItemViewModel, IFieldsObserver
    {
        private OctaneVersion octaneVersion;
        
        private ObservableCollection<CommentViewModel> _commentViewModels;
        private readonly List<FieldViewModel> _allEntityFields;

        private readonly List<Phase> _phaseTransitions;

        private string _filter = string.Empty;

        private bool _selectIsEnabled;

        private bool _saveIsEnabled;

        private string _commentText;

        internal static readonly string TempPath = Path.GetTempPath() + "\\Octane_pictures\\";

        private static readonly string lockStamp = "client_lock_stamp";
        /// <summary>
        /// Lets you enable or disable the phase ComboBox
        /// </summary>
        public bool SelectIsEnabled
        {
            get
            {
                return this._selectIsEnabled;
            }
            set
            {
                if (this._selectIsEnabled != value)
                {
                    this._selectIsEnabled = value;
                    NotifyPropertyChanged("SelectIsEnabled");
                }
            }
        }


        public override string Title
        {
            get
            {
                return base.Title;
            }
            set
            {
                base.Title = value;
                SaveIsEnabled = true;
            }
        }
        
        /// <summary>
        /// Lets you enable or disable the save Button
        /// </summary>
        public bool SaveIsEnabled
        {
            get
            {
                return this._saveIsEnabled;
            }
            set
            {
                if (this._saveIsEnabled != value)
                {
                    this._saveIsEnabled = value;
                    NotifyPropertyChanged("SaveIsEnabled");
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public DetailedItemViewModel(BaseEntity entity)
            : base(entity)
        {
            RefreshCommand = new DelegatedCommand(Refresh);
            SaveEntityCommand = new DelegatedCommand(SaveEntity);
            OpenInBrowserCommand = new DelegatedCommand(OpenInBrowser);
            ToggleCommentSectionCommand = new DelegatedCommand(SwitchCommentSectionVisibility);
            AddCommentCommand = new DelegatedCommand(AddComment);
            ToggleEntityFieldVisibilityCommand = new DelegatedCommand(ToggleEntityFieldVisibility);
            ResetFieldsCustomizationCommand = new DelegatedCommand(ResetFieldsCustomization);

            _commentViewModels = new ObservableCollection<CommentViewModel>();
            _allEntityFields = new List<FieldViewModel>();

            _phaseTransitions = new List<Phase>();

            Mode = WindowMode.Loading;

            EntitySupportsComments = EntityTypesSupportingComments.Contains(Utility.GetConcreteEntityType(entity));

            Id = (long)entity.Id;
            EntityType = Utility.GetConcreteEntityType(Entity);
            FieldsCache.Instance.Attach(this);
        }

        /// <summary>
        /// Initialize detailed information about the cached entity
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                OctaneServices octaneService = OctaneServices.GetInstance();

                List<FieldMetadata> fields = await FieldsMetadataService.GetFieldMetadata(Entity);
                List<string> fieldNames = fields.Select(fm => fm.Name).ToList();

                OctaneServices _octaneService = OctaneServices.GetInstance();

                if(octaneVersion == null)
                {
                    octaneVersion = await _octaneService.GetOctaneVersion();
                }

                // client lock stamp was introduced in octane 12.55.8
                if(octaneVersion.CompareTo(OctaneVersion.FENER_P3) > 0)
                {
                    // add client lock stamp to the fields that we want to retrieve
                    fieldNames.Add(lockStamp);

                }

                Entity = await _octaneService.FindEntityAsync(Entity, fieldNames);

                await HandleImagesInDescription();

                _allEntityFields.Clear();

                var visibleFieldsHashSet = FieldsCache.Instance.GetVisibleFieldsForEntity(EntityType);
                var fieldsToHideHashSet = FieldsCache.GetFieldsToHide(Entity);
                foreach (var field in fields.Where(f => !fieldsToHideHashSet.Contains(f.Name)))
                {
                    var fieldViewModel = new FieldViewModel(Entity, field, visibleFieldsHashSet.Contains(field.Name));
                    // add change handler to field view model
                    fieldViewModel.ChangeHandler += (sender, e) =>
                    {
                        SaveIsEnabled = true;
                    };
                    if (!string.Equals(fieldViewModel.Metadata.FieldType, "memo", StringComparison.OrdinalIgnoreCase))
                    {
                        _allEntityFields.Add(fieldViewModel);
                    }

                }
                if (EntitySupportsComments)
                    await RetrieveComments();

                var transitions = await octaneService.GetTransitionsForEntityType(EntityType);
                if (Entity.TypeName != "run" && Entity.TypeName != "run_manual" && Entity.TypeName != "run_suite")
                {

                    var phaseEntity = Entity.GetValue(CommonFields.Phase) as BaseEntity;
                    var currentPhaseName = phaseEntity.Name;

                    _phaseTransitions.Clear();
                    SelectedNextPhase = null;

                    foreach (var transition in transitions.Where(t => t.SourcePhase.Name == currentPhaseName))
                    {
                        if (transition.IsPrimary)
                            _phaseTransitions.Insert(0, transition.TargetPhase);
                        else
                            _phaseTransitions.Add(transition.TargetPhase);
                    }
                    this._selectIsEnabled = _phaseTransitions.Count != 0;
                }
                Mode = WindowMode.Loaded;
            }
            catch (Exception ex)
            {
                Mode = WindowMode.FailedToLoad;
                ErrorMessage = ex.Message;
            }
            NotifyPropertyChanged();
        }

        private async Task HandleImagesInDescription()
        {
            List<Task> downloadTasks = new List<Task>();
            List<Tuple<Element, string>> imageNodes = new List<Tuple<Element, string>>();
            bool needToUpdateDescription = false;
            Document doc = null;
            try
            {
                var description = Entity.GetStringValue(CommonFields.Description);
                if (string.IsNullOrEmpty(description))
                    return;

                if (!Directory.Exists(TempPath))
                {
                    Directory.CreateDirectory(TempPath);
                }

                doc = NSoup.Parse.Parser.Parse(description, OctaneConfiguration.Url);
                foreach (var image in doc.Select("img"))
                {
                    var relativeUrl = image.Attr("src");

                    if (relativeUrl == null || !relativeUrl.Contains("/api/shared_spaces"))
                        continue;

                    relativeUrl = relativeUrl.Substring(relativeUrl.IndexOf("/api/shared_spaces"));

                    var imageName = relativeUrl.Split('/').LastOrDefault();
                    if (string.IsNullOrEmpty(imageName))
                        continue;

                    var imagePath = TempPath + EntityType + Entity.Id + imageName;
                    if (!File.Exists(imagePath))
                    {
                        OctaneServices octaneService = OctaneServices.GetInstance();

                        imageNodes.Add(new Tuple<Element, string>(image, imagePath));
                        downloadTasks.Add(octaneService.DownloadAttachmentAsync(relativeUrl, imagePath));
                    }
                    else
                    {
                        image.Attr("src", imagePath);
                    }

                    needToUpdateDescription = true;
                }

                await Task.WhenAll(downloadTasks);
            }
            catch (Exception)
            {
            }

            for (var i = 0; i < downloadTasks.Count; i++)
            {
                if (downloadTasks[i].Status == TaskStatus.RanToCompletion)
                {
                    imageNodes[i].Item1.Attr("src", imageNodes[i].Item2);
                }
            }

            if (doc != null && needToUpdateDescription)
            {
                Entity.SetValue(CommonFields.Description, doc.ToString());
            }
        }

        /// <summary>
        /// Flag specifying whether the current entity supports comments
        /// </summary>
        public bool EntitySupportsComments { get; }

        #region IFieldsObserver

        /// <inheritdoc />
        public long Id { get; }

        /// <inheritdoc />
        public string EntityType { get; }

        /// <inheritdoc />
        public void UpdateFields()
        {
            var persistedVisibleFields = FieldsCache.Instance.GetVisibleFieldsForEntity(EntityType);
            foreach (var field in _allEntityFields)
            {
                field.IsSelected = persistedVisibleFields.Contains(field.Name);
            }

            NotifyPropertyChanged("FilteredEntityFields");
            NotifyPropertyChanged("VisibleFields");
        }

        #endregion

        /// <summary>
        /// Entity fields shown in the main section of the detailed item view
        /// </summary>
        public IEnumerable<FieldViewModel> VisibleFields
        {
            get
            {
                NotifyPropertyChanged("OnlyDefaultFieldsAreShown");
                return new ObservableCollection<FieldViewModel>(_allEntityFields.Where(f => f.IsSelected));
            }
        }

        #region FieldCustomization

        /// <summary>
        /// Entity fields shown in the field customization control after the search filter has been applied
        /// </summary>
        public IEnumerable<FieldViewModel> FilteredEntityFields
        {
            get
            {
                return new ObservableCollection<FieldViewModel>(
                    _allEntityFields.Where(f => f.Label.ToLowerInvariant().Contains(_filter))
                                    .OrderBy(f => f.Label));
            }
        }

        /// <summary>
        /// Search filter applied on the entity fields
        /// </summary>
        public string Filter
        {
            get { return _filter; }
            set
            {
                _filter = value?.ToLowerInvariant() ?? string.Empty;
                NotifyPropertyChanged("FilteredEntityFields");
            }
        }

        /// <summary>
        /// Command for handling the visibility change of an entity field
        /// </summary>
        public ICommand ToggleEntityFieldVisibilityCommand { get; }

        private void ToggleEntityFieldVisibility(object param)
        {
            FieldsCache.Instance.UpdateVisibleFieldsForEntity(EntityType, _allEntityFields);
        }

        /// <summary>
        /// Command for reseting the visible fields to the defaults for the current entity
        /// </summary>
        public ICommand ResetFieldsCustomizationCommand { get; }

        private void ResetFieldsCustomization(object param)
        {
            FieldsCache.Instance.ResetVisibleFieldsForEntity(EntityType);
        }

        /// <summary>
        /// Flag specifying whether only the default fields for the current entity are selected and shown in the view
        /// </summary>
        public bool OnlyDefaultFieldsAreShown
        {
            get
            {
                return FieldsCache.Instance.AreSameFieldsAsDefaultFields(EntityType,
                    _allEntityFields.Where(f => f.IsSelected).ToList());
            }
        }

        #endregion

        /// <summary>
        /// Message shown to the user in case of an error
        /// </summary>
        public string ErrorMessage { get; private set; }

        private async Task RetrieveComments()
        {
            try
            {
                var viewModels = new List<CommentViewModel>();

                OctaneServices octaneServices = OctaneServices.GetInstance();

                var commentEntities = await octaneServices.GetAttachedCommentsToEntity(Entity);
                foreach (var comment in commentEntities)
                {
                    viewModels.Add(new CommentViewModel(comment));
                }

                _commentViewModels = new ObservableCollection<CommentViewModel>(viewModels.OrderByDescending(c =>
                {
                    try
                    {
                        return DateTime.ParseExact(c.CreationTime, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                    }
                    catch (Exception)
                    {
                        return DateTime.Now;
                    }
                }));
            }
            catch (Exception)
            {
                _commentViewModels.Clear();
            }
        }

        #region Phase

        /// <summary>
        /// Flag specifiying whether we need to show the phase section in the UI or not
        /// </summary>
        public bool ShowPhase
        {
            get { return !(Entity is Run); }
        }

        /// <summary>
        /// Current phase for the entity
        /// </summary>
        public string Phase
        {
            get
            {
                var phaseEntity = Entity.GetValue(CommonFields.Phase) as BaseEntity;
                if (phaseEntity == null)
                    return string.Empty;

                return phaseEntity.Name;                
            }
        }

        /// <summary>
        /// List of names for possible next phases
        /// </summary>
        public List<string> NextPhaseNames
        {
            get { return _phaseTransitions.Select(pt => pt.Name).ToList(); }
        }


        private string _selectedNextPhase;
        /// <summary>
        /// Name of the currently selected next phase
        /// </summary>
        public string SelectedNextPhase {
            get
            {
                return _selectedNextPhase;
            }
            set
            {
                if(value != null)
                {
                    SaveIsEnabled = true;
                }
                _selectedNextPhase = value;
                NotifyPropertyChanged("SelectedNextPhase");
            }
        }

        #endregion

        /// <summary>
        /// Comments associated with the cached entity
        /// </summary>
        public IEnumerable<CommentViewModel> Comments
        {
            get { return _commentViewModels; }
        }

        /// <summary>
        /// Current status of the detailed view
        /// </summary>
        public WindowMode Mode { get; private set; }

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
            Requirement.SUBTYPE_DOCUMENT,

            // task
            TaskEntity.TYPE_TASK
        };

        #region SaveEntity

        /// <summary>
        /// Save entity command
        /// </summary>
        public ICommand SaveEntityCommand { get; }

        private async void SaveEntity(object param)
        {
			try
			{
				Mode = WindowMode.Loading;
				NotifyPropertyChanged("Mode");

				var entityToUpdate = new BaseEntity(Entity.Id);
				entityToUpdate.SetValue(BaseEntity.TYPE_FIELD, Entity.TypeName);

				// add the client lock stamp if it exists
				if (Entity.GetLongValue(lockStamp) != null)
				{
					entityToUpdate.SetLongValue(lockStamp, (long)Entity.GetLongValue(lockStamp));
				}

				foreach (var field in _allEntityFields.Where(f => f.IsChanged))
				{
					if (!field.Metadata.FieldType.Equals("reference"))
					{
						if ("".Equals(field.Content))
						{
							entityToUpdate.SetValue(field.Name, null);
						}
						else
						{
							entityToUpdate.SetValue(field.Name, field.Content);
						}
					}
					else if (!field.IsMultiple)
					{
						if (field.Content == null || field.Content.Equals(""))
						{
							entityToUpdate.SetValue(field.Name, null);
						}
						else
						{
							foreach (BaseEntity be in field.ReferenceFieldContentBaseEntity)
							{
								// todo: you need to look into this tibi
								if (field.Content.Equals(new BaseEntityWrapper(be)))
								{
									entityToUpdate.SetValue(field.Name, be);
								}
							}

						}

					}
					else if (field.IsMultiple)
					{
						entityToUpdate.SetValue(field.Name, field.GetSelectedEntities());
					}
				}

				entityToUpdate.Name = Entity.Name;

				if (SelectedNextPhase != null)
				{
					var nextPhase = _phaseTransitions.FirstOrDefault(t => t.Name == SelectedNextPhase);
					if (nextPhase != null)
						entityToUpdate.SetValue(CommonFields.Phase, nextPhase);
				}

				await OctaneServices.GetInstance().UpdateEntityAsync(entityToUpdate);
				await InitializeAsync();

				//trigger a refresh after save so the user is aware of the changes
				RefreshCommand.Execute(param);
				NotifyPropertyChanged();
                // disable save button until field values update
                _saveIsEnabled = false;
			}
			catch (Exception ex)
			{
				BusinessErrorDialog bed = new BusinessErrorDialog(this, (MqmRestException)ex);
				bed.ShowDialog();
			}
			finally
			{
				Mode = WindowMode.Loaded;
				NotifyPropertyChanged("Mode");
			}
           
        }

        #endregion

        #region AddComments

        public ICommand AddCommentCommand { get; private set; }

        public string CommentText
        {
            get { return _commentText; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    value = value.TrimStart();
                }

                if (value != _commentText)
                {
                    if (value.Contains("\n"))
                    {
                        string valueTrimmed = value.Replace("\n", "<br>");
                        value = valueTrimmed;
                    }
                    _commentText = value;
                    NotifyPropertyChanged("CommentText");
                }
            }
        }


        private async void AddComment(object param)
        {
            try
            {
                Mode = WindowMode.LoadingComments;
                NotifyPropertyChanged("Mode");
                if (EntitySupportsComments)
                {
                    await AddCommentAsync();
                    NotifyPropertyChanged();
                    await RetrieveComments();
                }
            }
            catch (Exception ex)
            {
                Mode = WindowMode.FailedToLoad;
                ErrorMessage = ex.Message;
            }
            Mode = WindowMode.Loaded;
            NotifyPropertyChanged();
        }

        public async Task AddCommentAsync()
        {
            var commentToAdd = new Api.Core.Entities.Comment();
            string encodedCommment = "<html><body>" + CommentText + "</body></html>";
            commentToAdd.Text = encodedCommment;

            if (Entity.TypeName == "task") // task does not have aggregate type
            {              
                TaskEntity commentOwnerTask = new TaskEntity();
                commentOwnerTask.Id = Entity.Id;
                commentOwnerTask.TypeName = Entity.TypeName;
                commentToAdd.OwnerTask = commentOwnerTask;
            }
            else
            {
                string entityAggregateType = Entity.AggregateType;

                switch (entityAggregateType)
                {
                    case "work_item":
                        BaseEntity commentOwnerWorkItem = new BaseEntity();
                        commentOwnerWorkItem.Id = Entity.Id;
                        commentOwnerWorkItem.TypeName = Entity.TypeName;
                        commentToAdd.OwnerWorkItem = commentOwnerWorkItem;
                        break;
                    case "test":
                        Test commentOwnerTest = new Test();
                        commentOwnerTest.Id = Entity.Id;
                        commentOwnerTest.TypeName = Entity.TypeName;
                        commentToAdd.OwnerTest = commentOwnerTest;
                        break;
                    case "requirement":
                        Requirement commentOwnerRequirement = new Requirement();
                        commentOwnerRequirement.Id = Entity.Id;
                        commentOwnerRequirement.TypeName = Entity.TypeName;
                        commentToAdd.OwnerRequirement = commentOwnerRequirement;
                        break;
                    case "run":
                        Run commentOwnerRun = new Run();
                        commentOwnerRun.Id = Entity.Id;
                        commentOwnerRun.TypeName = Entity.TypeName;
                        commentToAdd.OwnerRun = commentOwnerRun;
                        break;
                    default:
                        break;
                }
            }
            CommentText = "";

            await OctaneServices.GetInstance().CreateCommentAsync(commentToAdd);
            NotifyPropertyChanged();
        }

        #endregion

        #region Refresh

        /// <summary>
        /// Refresh entity information
        /// </summary>
        public ICommand RefreshCommand { get; }

        private void Refresh(object param)
        {
            try
            {
                Mode = WindowMode.Loading;
                NotifyPropertyChanged("Mode");

                InitializeAsync();
                //disable the save button once the entity has been refreshed
                _saveIsEnabled = false;
            }
            catch (Exception ex)
            {
                Mode = WindowMode.FailedToLoad;
                ErrorMessage = ex.Message;
            }
            NotifyPropertyChanged();
        }

        #endregion

        #region OpenInBrowser

        /// <summary>
        /// Open in browser command
        /// </summary>
        public ICommand OpenInBrowserCommand { get; }

        private void OpenInBrowser(object param)
        {
            Utility.OpenInBrowser(Entity);
        }

        #endregion

        #region CommentSectionVisibility

        /// <summary>
        /// Flag specifying whether the comment section is visible or not
        /// </summary>
        public bool CommentSectionVisibility { get; set; }

        /// <summary>
        /// Toggle the comment section visibility command
        /// </summary>
        public ICommand ToggleCommentSectionCommand { get; }

        private void SwitchCommentSectionVisibility(object param)
        {
            CommentSectionVisibility = !CommentSectionVisibility;
            NotifyPropertyChanged("CommentSectionVisibility");
            NotifyPropertyChanged("ShowCommentTooltip");
        }

        internal const string HideCommentsTooltip = "Hide comments";
        internal const string ShowCommentsTooltip = "Show comments";

        /// <summary>
        /// Comment button tooltip
        /// </summary>
        public string ShowCommentTooltip
        {
            get { return CommentSectionVisibility ? HideCommentsTooltip : ShowCommentsTooltip; }
        }

        #endregion
    }
}
