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

        public DetailedItemViewModel(BaseEntity entity, MyWorkMetadata myWorkMetadata)
            : base(entity, myWorkMetadata)
        {
            RefreshCommand = new DelegatedCommand(Refresh);
            ToggleCommentSectionCommand = new DelegatedCommand(SwitchCommentSectionVisibility);

            _commentViewModels = new ObservableCollection<CommentViewModel>();

            Mode = DetailsWindowMode.LoadingItem;

            if (EntityTypesSupportingComments.Contains(Utility.GetConcreteEntityType(entity)))
                EntitySupportsComments = true;

            _octaneService = new OctaneServices(
                OctaneMyItemsViewModel.Instance.Package.AlmUrl,
                OctaneMyItemsViewModel.Instance.Package.SharedSpaceId,
                OctaneMyItemsViewModel.Instance.Package.WorkSpaceId,
                OctaneMyItemsViewModel.Instance.Package.AlmUsername,
                OctaneMyItemsViewModel.Instance.Package.AlmPassword);
        }

        public async void Initialize()
        {
            try
            {
                await _octaneService.Connect();
                Entity = await _octaneService.FindEntity(Entity);

                if (EntitySupportsComments)
                    await RetrieveComments();

                Mode = DetailsWindowMode.ItemLoaded;
                NotifyPropertyChanged();
            }
            catch (Exception ex)
            {
                Mode = DetailsWindowMode.FailedToLoad;
                ErrorMessage = ex.Message;
            }
            NotifyPropertyChanged();
        }

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
