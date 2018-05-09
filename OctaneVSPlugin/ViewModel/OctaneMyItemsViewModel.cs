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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace MicroFocus.Adm.Octane.VisualStudio.ViewModel
{
    /// <summary>
    /// View model for entities directly related to the current user
    /// </summary>
    public class OctaneMyItemsViewModel : INotifyPropertyChanged
    {
        private MainWindowMode _mode;
        private readonly ObservableCollection<OctaneItemViewModel> _myItems;

        /// <summary>
        /// Store the exception message from the loading items operation
        /// </summary>
        private string _lastExceptionMessage;

        private const int MaxSearchHistorySize = 4;
        private readonly List<string> _searchHistory;

        /// <summary>
        /// Constructor
        /// </summary>
        public OctaneMyItemsViewModel()
        {
            Instance = this;

            SearchCommand = new DelegateCommand(SearchInternal);
            RefreshCommand = new DelegatedCommand(Refresh);
            OpenOctaneOptionsDialogCommand = new DelegatedCommand(OpenOctaneOptionsDialog);

            try
            {
                _searchHistory = OctanePluginSettings.Default.SearchHistory.Cast<string>().ToList();
            }
            catch (Exception)
            {
                _searchHistory = new List<string>();
            }

            _myItems = new ObservableCollection<OctaneItemViewModel>();
            _mode = MainWindowMode.FirstTime;
        }

        public static OctaneMyItemsViewModel Instance { get; private set; }

        /// <summary>
        /// Current state of the view model
        /// </summary>
        public MainWindowMode Mode
        {
            get { return _mode; }
            private set
            {
                _mode = value;
                NotifyPropertyChanged("Mode");
            }
        }

        #region Search

        /// <summary>
        /// Current search filter
        /// </summary>
        public string SearchFilter { get; set; }

        public ICommand SearchCommand { get; }

        private void SearchInternal(object parameter)
        {
            if (string.IsNullOrEmpty(SearchFilter))
                return;

            UpdateSearchHistory();

            var searchWindow = PluginWindowManager.ObtainSearchWindow(MainWindow.PluginPackage);
            searchWindow.Search(SearchFilter);
        }

        private void UpdateSearchHistory()
        {
            var oldHistory = _searchHistory.ToList();

            _searchHistory.Clear();
            _searchHistory.Add(SearchFilter);

            oldHistory.Remove(SearchFilter);

            _searchHistory.AddRange(oldHistory.Take(MaxSearchHistorySize));

            try
            {
                var sc = new StringCollection();
                sc.AddRange(_searchHistory.ToArray());
                OctanePluginSettings.Default.SearchHistory = sc;
                OctanePluginSettings.Default.Save();
            }
            catch (Exception)
            {
            }

            NotifyPropertyChanged("SearchHistory");
        }

        /// <summary>
        /// Enumeration of the last searches
        /// </summary>
        public IEnumerable<string> SearchHistory
        {
            get
            {
                return new ObservableCollection<string>(_searchHistory);
            }
        }

        #endregion

        /// <summary>
        /// Enumeration containing entities related to the current user
        /// </summary>
        public IEnumerable<OctaneItemViewModel> MyItems
        {
            get { return _myItems; }
        }

        /// <summary>
        /// Error message
        /// </summary>
        public string LastExceptionMessage
        {
            get { return _lastExceptionMessage; }
            set
            {
                _lastExceptionMessage = value;
                NotifyPropertyChanged("LastExceptionMessage");
            }
        }

        /// <summary>
        /// Refresh command
        /// </summary>
        public ICommand RefreshCommand { get; }

        private void Refresh(object parameter)
        {
            LoadMyItemsAsync();
        }

        /// <summary>
        /// Command for opening the ALM Octane options dialog
        /// </summary>
        public ICommand OpenOctaneOptionsDialogCommand { get; }

        private void OpenOctaneOptionsDialog(object parameter)
        {
            MainWindow.PluginPackage.ShowOptionPage(typeof(OptionsPage));
        }

        /// <summary>
        /// Retrieve all the entities related to the current user
        /// </summary>
        internal async System.Threading.Tasks.Task LoadMyItemsAsync()
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

                _myItems.Clear();

                IList<BaseEntity> items = await octane.GetMyItems();
                foreach (BaseEntity entity in items)
                {
                    _myItems.Add(new OctaneItemViewModel(entity));
                }

                IList<BaseEntity> comments = await octane.GetMyCommentItems();
                foreach (BaseEntity comment in comments)
                {
                    _myItems.Add(new CommentViewModel(comment));
                }

                Mode = MainWindowMode.ItemsLoaded;
            }
            catch (Exception ex)
            {
                Mode = MainWindowMode.FailToLoad;
                LastExceptionMessage = ex.Message;
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        #endregion
    }
}
