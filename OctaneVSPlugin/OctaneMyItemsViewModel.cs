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
using octane_visual_studio_plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace MicroFocus.Adm.Octane.VisualStudio
{
    public class OctaneMyItemsViewModel : INotifyPropertyChanged
    {
        private static OctaneMyItemsViewModel instance;

        private DelegatedCommand refreshCommand;
        private DelegatedCommand openOctaneOptionsDialogCommand;
        private MainWindowPackage package;

        private MainWindowMode mode;
        private ObservableCollection<OctaneItemViewModel> myItems;

        private MyWorkMetadata myWorkMetadata;

        /// <summary>
        /// Store the exception message from load items.
        /// </summary>
        private string lastExceptionMessage;

        public event PropertyChangedEventHandler PropertyChanged;

        public OctaneMyItemsViewModel()
        {
            instance = this;

            myWorkMetadata = new MyWorkMetadata();

            refreshCommand = new DelegatedCommand(Refresh);
            openOctaneOptionsDialogCommand = new DelegatedCommand(OpenOctaneOptionsDialog);
            myItems = new ObservableCollection<OctaneItemViewModel>();
            mode = MainWindowMode.FirstTime;
        }

        public static OctaneMyItemsViewModel Instance
        {
            get { return instance; }
        }

        public MainWindowMode Mode
        {
            get { return mode; }
            private set
            {
                mode = value;
                NotifyPropertyChanged("Mode");
            }
        }

        public ICommand RefreshCommand
        {
            get { return refreshCommand; }
        }

        public ICommand OpenOctaneOptionsDialogCommand
        {
            get { return openOctaneOptionsDialogCommand; }
        }

        public IEnumerable<OctaneItemViewModel> MyItems
        {
            get { return myItems; }
        }

        public string LastExceptionMessage
        {
            get { return lastExceptionMessage; }
            set
            {
                lastExceptionMessage = value;
                NotifyPropertyChanged("LastExceptionMessage");
            }
        }

        private void Refresh(object parameter)
        {
            LoadMyItems();
        }

        private void OpenOctaneOptionsDialog(object parameter)
        {
            package.ShowOptionPage(typeof(OptionsPage));
        }

        /// <summary>
        /// This method is called after the options are changed.
        /// </summary>
        internal void OptionsChanged()
        {
            LoadMyItems();
        }

        internal async void LoadMyItems()
        {
            if (string.IsNullOrEmpty(package.AlmUrl))
            {
                // No URL which means it's the first time use.
                Mode = MainWindowMode.FirstTime;
                return;
            }

            // If we already loading wait for the runnung load to complete.
            if (Mode == MainWindowMode.LoadingItems)
            {
                return;
            }

            Mode = MainWindowMode.LoadingItems;

            try
            {
                OctaneServices octane = new OctaneServices(package.AlmUrl, package.SharedSpaceId, package.WorkSpaceId, package.AlmUsername, package.AlmPassword);
                await octane.Connect();

                myItems.Clear();

                IList<BaseEntity> items = await octane.GetMyItems(myWorkMetadata);
                foreach (BaseEntity entity in items)
                {
                    myItems.Add(new OctaneItemViewModel(entity, myWorkMetadata));
                }

                IList<BaseEntity> comments = await octane.GetMyCommentItems();
                foreach (BaseEntity comment in comments)
                {
                    myItems.Add(new CommentViewModel(comment, myWorkMetadata));
                }

                Mode = MainWindowMode.ItemsLoaded;
            }
            catch (Exception ex)
            {
                Mode = MainWindowMode.FailToLoad;
                LastExceptionMessage = ex.Message;
            }
        }

        internal void SetPackage(MainWindowPackage package)
        {
            this.package = package;
            LoadMyItems();
        }

        internal MainWindowPackage Package
        {
            get { return package; }
        }

        private void NotifyPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        public async System.Threading.Tasks.Task<string> GetGherkinScript(Test test)
        {
            try
            {
                OctaneServices octane = new OctaneServices(package.AlmUrl, package.SharedSpaceId, package.WorkSpaceId, package.AlmUsername, package.AlmPassword);
                await octane.Connect();

                TestScript testScript = await octane.GetTestScript(test.Id);
                return testScript.Script;
            }
            catch (Exception ex)
            {
                throw new Exception("Fail to get test script", ex);
            }
        }

        internal async System.Threading.Tasks.Task<OctaneItemViewModel> GetItem(BaseEntity entityModel)
        {
            OctaneServices octane = new OctaneServices(package.AlmUrl, package.SharedSpaceId, package.WorkSpaceId, package.AlmUsername, package.AlmPassword);
            await octane.Connect();

            var entity = await octane.FindEntity(entityModel);
            return new OctaneItemViewModel(entity, myWorkMetadata);
        }
    }
}
