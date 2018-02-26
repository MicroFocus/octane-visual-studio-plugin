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
using System.ComponentModel;
using System.Threading.Tasks;
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

        public DetailedItemViewModel(BaseEntity entity, MyWorkMetadata myWorkMetadata)
            : base(entity, myWorkMetadata)
        {
            _toggleCommentSectionCommand = new DelegatedCommand(SwitchCommentSectionVisibility);

            Mode = MainWindowMode.LoadingItems;
        }

        public async void Initialize()
        {
            Entity = await GetItem(Entity);
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

        private async Task<BaseEntity> GetItem(BaseEntity entityModel)
        {
            OctaneServices octane = new OctaneServices(
                OctaneMyItemsViewModel.Instance.Package.AlmUrl,
                OctaneMyItemsViewModel.Instance.Package.SharedSpaceId,
                OctaneMyItemsViewModel.Instance.Package.WorkSpaceId,
                OctaneMyItemsViewModel.Instance.Package.AlmUsername,
                OctaneMyItemsViewModel.Instance.Package.AlmPassword);
            await octane.Connect();

            return await octane.FindEntity(entityModel);
        }

        public MainWindowMode Mode { get; private set; }

        public bool CommentSectionVisibility { get; set; }

        public ICommand ToggleCommentSectionCommand
        {
            get { return _toggleCommentSectionCommand; }
        }

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
