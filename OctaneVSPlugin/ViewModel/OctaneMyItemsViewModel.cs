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
using MicroFocus.Adm.Octane.VisualStudio.View;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace MicroFocus.Adm.Octane.VisualStudio.ViewModel
{
    public class OctaneMyItemsViewModel : INotifyPropertyChanged
    {
        private static OctaneMyItemsViewModel instance;

        private DelegatedCommand refreshCommand;
        private DelegatedCommand openOctaneOptionsDialogCommand;

        private MainWindowMode mode;
        private ObservableCollection<OctaneItemViewModel> myItems;

        /// <summary>
        /// Store the exception message from load items.
        /// </summary>
        private string lastExceptionMessage;

        public event PropertyChangedEventHandler PropertyChanged;

        public OctaneMyItemsViewModel()
        {
            instance = this;

            SearchCommand = new DelegateCommand(SearchInternal);
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

        #region Search

        public string SearchFilter { get; set; }

        public ICommand SearchCommand { get; }

        private void SearchInternal(object parameter)
        {
            if (string.IsNullOrEmpty(SearchFilter))
                return;

            var oldHistory = _searchHistory.ToList();

            _searchHistory.Clear();
            _searchHistory.Add(SearchFilter);

            oldHistory.Remove(SearchFilter);

            _searchHistory.AddRange(oldHistory.Take(MaxSearchHistorySize));
            NotifyPropertyChanged("SearchHistory");

            // TODO Compute unique ID for search window
            SearchToolWindow searchWindow = (SearchToolWindow)MainWindow.PluginPackage.FindToolWindow(typeof(SearchToolWindow), 100000, true);
            if (searchWindow?.Frame == null)
            {
                throw new NotSupportedException("Cannot create search tool window");
            }
            IVsWindowFrame searchWindowFrame = (IVsWindowFrame)searchWindow.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(searchWindowFrame.Show());
            searchWindow.Search(SearchFilter);
        }

        private const int MaxSearchHistorySize = 4;
        // TODO replace with persisted history
        private static List<string> _searchHistory = new List<string> { "a", "b", "c" };

        public IEnumerable<string> SearchHistory
        {
            get
            {
                return new ObservableCollection<string>(_searchHistory);
            }
        }

        #endregion

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
            MainWindow.PluginPackage.ShowOptionPage(typeof(OptionsPage));
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
            if (string.IsNullOrEmpty(OctaneConfiguration.Url))
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
                OctaneServices octane = new OctaneServices(OctaneConfiguration.Url,
                    OctaneConfiguration.SharedSpaceId,
                    OctaneConfiguration.WorkSpaceId,
                    OctaneConfiguration.Username,
                    OctaneConfiguration.Password);
                await octane.Connect();

                myItems.Clear();

                IList<BaseEntity> items = await octane.GetMyItems();
                foreach (BaseEntity entity in items)
                {
                    myItems.Add(new OctaneItemViewModel(entity));
                }

                IList<BaseEntity> comments = await octane.GetMyCommentItems();
                foreach (BaseEntity comment in comments)
                {
                    myItems.Add(new CommentViewModel(comment));
                }

                Mode = MainWindowMode.ItemsLoaded;
            }
            catch (Exception ex)
            {
                Mode = MainWindowMode.FailToLoad;
                LastExceptionMessage = ex.Message;
            }
        }

        private void NotifyPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
    }
}
