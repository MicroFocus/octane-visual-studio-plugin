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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;

namespace MicroFocus.Adm.Octane.VisualStudio.ViewModel
{
    /// <summary>
    /// Detailed view model for an entity
    /// </summary>
    public class DetailedItemViewModel : BaseItemViewModel, INotifyPropertyChanged
    {
        private readonly DelegatedCommand _toggleCommentSectionCommand;
        private readonly OctaneServices _octaneService;

        private ObservableCollection<CommentViewModel> _commentViewModels;

        public DetailedItemViewModel(BaseEntity entity, MyWorkMetadata myWorkMetadata)
            : base(entity, myWorkMetadata)
        {
            _toggleCommentSectionCommand = new DelegatedCommand(SwitchCommentSectionVisibility);

            _commentViewModels = new ObservableCollection<CommentViewModel>();

            Mode = MainWindowMode.LoadingItems;

            _octaneService = new OctaneServices(
                OctaneMyItemsViewModel.Instance.Package.AlmUrl,
                OctaneMyItemsViewModel.Instance.Package.SharedSpaceId,
                OctaneMyItemsViewModel.Instance.Package.WorkSpaceId,
                OctaneMyItemsViewModel.Instance.Package.AlmUsername,
                OctaneMyItemsViewModel.Instance.Package.AlmPassword);
        }

        public async void Initialize()
        {
            await _octaneService.Connect();
            Entity = await _octaneService.FindEntity(Entity);
            Mode = MainWindowMode.ItemsLoaded;
            NotifyPropertyChanged();
        }

        public override string Description
        {
            get { return Mode != MainWindowMode.LoadingItems ? Entity.GetStringValue(CommonFields.DESCRIPTION) ?? string.Empty : string.Empty; }
        }

        public override string IconText
        {
            get { return Mode != MainWindowMode.LoadingItems ? MyWorkMetadata.GetIconText(Entity) : null; }
        }

        public override Color IconBackgroundColor
        {
            get { return Mode != MainWindowMode.LoadingItems ? MyWorkMetadata.GetIconColor(Entity) : new Color(); }
        }

        public IEnumerable<CommentViewModel> Comments
        {
            get { return _commentViewModels; }
        }

        public MainWindowMode Mode { get; private set; }

        public bool CommentSectionVisibility { get; set; }

        public ICommand ToggleCommentSectionCommand
        {
            get { return _toggleCommentSectionCommand; }
        }

        private async void SwitchCommentSectionVisibility(object param)
        {
            CommentSectionVisibility = !CommentSectionVisibility;

            if (CommentSectionVisibility)
            {
                try
                {
                    _commentViewModels.Clear();
                    var commentEntities = await _octaneService.GetAttachedCommentsToEntity(Entity);
                    foreach (var comment in commentEntities)
                    {
                        _commentViewModels.Add(new CommentViewModel(comment, MyWorkMetadata));
                    }
                }
                catch (Exception)
                {
                    _commentViewModels.Clear();
                }
            }
            else
            {
                _commentViewModels.Clear();
            }
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
