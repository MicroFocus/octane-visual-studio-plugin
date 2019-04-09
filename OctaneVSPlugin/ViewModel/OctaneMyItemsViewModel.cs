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
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using MicroFocus.Adm.Octane.Api.Core.Connector.Exceptions;

namespace MicroFocus.Adm.Octane.VisualStudio.ViewModel
{
    /// <summary>
    /// View model for entities directly related to the current user
    /// </summary>
    public class OctaneMyItemsViewModel : INotifyPropertyChanged
    {
        private MainWindowMode _mode;
        private readonly ObservableCollection<OctaneItemViewModel> _myItems;

        private List<MyWorkItemsSublist> myWorkItemSublists;
        private Dictionary<string, MyWorkItemsSublist> sublistsMap;

        /// <summary>
        /// Store the exception message from the loading items operation
        /// </summary>
        private string _lastExceptionMessage;

        /// <summary>
        /// Constructor
        /// </summary>
        public OctaneMyItemsViewModel()
        {
            Instance = this;

            SearchCommand = new DelegateCommand(SearchInternal);
            RefreshCommand = new DelegatedCommand(Refresh);
            OpenOctaneOptionsDialogCommand = new DelegatedCommand(OpenOctaneOptionsDialog);

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
            set
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
            if (string.IsNullOrEmpty(SearchFilter) || string.IsNullOrEmpty(SearchFilter.Trim()))
                return;

            SearchFilter = SearchFilter.Trim();

            WorkspaceSessionPersistanceManager.UpdateHistory(SearchFilter);
            NotifyPropertyChanged("SearchHistory");

            PluginWindowManager.ShowSearchWindow(MainWindow.PluginPackage, SearchFilter);
        }

        /// <summary>
        /// Enumeration of the last searches
        /// </summary>
        public IEnumerable<string> SearchHistory
        {
            get
            {
                return new ObservableCollection<string>(_mode == MainWindowMode.ItemsLoaded
                           ? WorkspaceSessionPersistanceManager.History
                           : new List<string>());
            }
        }

        #endregion

        private int _totalItems;

        public int TotalItems
        {
            get { return _totalItems; }
        }

        /// <summary>
        /// Enumeration containing entities related to the current user
        /// </summary>
        public IEnumerable<OctaneItemViewModel> MyItems
        {
            get { return _myItems; }
        }

        public IEnumerable<MyWorkItemsSublist> MyWorkSublists
        {
            get { return myWorkItemSublists; }
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
            MainWindow.PluginPackage.ShowOptionPage(typeof(ConnectionSettings));
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
                OctaneServices octaneService;
                try
                {
                    octaneService = OctaneServices.GetInstance();
                } catch (Exception e)
                {
                    if (e.GetBaseException().Message.Equals("Object not created"))
                    {
                        OctaneServices.Create(OctaneConfiguration.Url,
                           OctaneConfiguration.SharedSpaceId,
                           OctaneConfiguration.WorkSpaceId);
                    }
                    octaneService = OctaneServices.GetInstance();
                    await octaneService.Connect();

                }

                _myItems.Clear();

                bool foundActiveItem = false;
                IList<BaseEntity> items = await octaneService.GetMyItems();

                if (sublistsMap == null)
                {
                    sublistsMap = createMyWorkItemsSublistsMap();
                }
                else
                {
                    cleanSubListsMap(sublistsMap);
                }
                
                foreach (BaseEntity entity in items)
                {
                    if("feature".Equals(entity.GetStringValue("subtype")))
                    {
                        continue;
                    }

                    var octaneItem = new OctaneItemViewModel(entity);
                    _totalItems++;
                    if (WorkspaceSessionPersistanceManager.IsActiveEntity(entity))
                    {
                        foundActiveItem = true;
                        OctaneItemViewModel.SetActiveItem(octaneItem);
                    }
                    MyWorkItemsSublist itemSublist;
                    if(sublistsMap.TryGetValue(Utility.GetConcreteEntityType(entity),out itemSublist))
                    {
                        itemSublist.Items.Add(octaneItem);
                    }
                }

                myWorkItemSublists = sublistsMap.Values.ToList();

                if (!foundActiveItem)
                {
                    OctaneItemViewModel.ClearActiveItem();
                    MainWindowCommand.Instance?.DisableActiveItemToolbar();
                }

                IList<BaseEntity> comments = await octaneService.GetMyCommentItems();
                foreach (BaseEntity comment in comments)
                {
                    _totalItems++;
                    MyWorkItemsSublist itemSublist;
                    if (sublistsMap.TryGetValue("comment", out itemSublist))
                    {
                        itemSublist.Items.Add(new CommentViewModel(comment));
                    }
                }

                myWorkItemSublists.ForEach(ms =>
                {
                    if(ms.IsSelected) {
                        foreach (var myWorkItem in ms.Items)
                        {
                            _myItems.Add(myWorkItem);
                        }
                    }
                });

                Mode = MainWindowMode.ItemsLoaded;

                SearchFilter = "";
                MainWindowCommand.Instance?.UpdateActiveItemInToolbar();
                NotifyPropertyChanged();
            }
            catch (Exception ex)
            {
                MainWindowCommand.Instance?.DisableActiveItemToolbar();
                Mode = MainWindowMode.FailToLoad;
                if(ex is NotConnectedException)
                {
                    LastExceptionMessage = "Failed to load \"My Work\"";
                }
                else
                {
                    LastExceptionMessage = ex.Message;
                }
               
            }
        }

        public void ApplyFilter()
        {
            _myItems.Clear();
            myWorkItemSublists.ForEach(ms =>
            {
                if (ms.IsSelected)
                {
                    foreach (var myWorkItem in ms.Items) _myItems.Add(myWorkItem);
                }
            });
        }

        private void cleanSubListsMap(Dictionary<string, MyWorkItemsSublist> sublistMap)
        {
            foreach (KeyValuePair<string, MyWorkItemsSublist> entry in sublistMap)
            {
                entry.Value.Items.Clear();
            }
        }

        private Dictionary<string, MyWorkItemsSublist> createMyWorkItemsSublistsMap()
        {
            return new Dictionary<string, MyWorkItemsSublist>
                {
                    { WorkItem.SUBTYPE_STORY, new MyWorkItemsSublist(WorkItem.SUBTYPE_STORY) },
                    { WorkItem.SUBTYPE_QUALITY_STORY, new MyWorkItemsSublist(WorkItem.SUBTYPE_QUALITY_STORY) },
                    { WorkItem.SUBTYPE_DEFECT, new MyWorkItemsSublist(WorkItem.SUBTYPE_DEFECT)},
                    { Task.TYPE_TASK, new MyWorkItemsSublist(Task.TYPE_TASK) },
                    { Requirement.SUBTYPE_DOCUMENT, new MyWorkItemsSublist(Requirement.SUBTYPE_DOCUMENT) },
                    { Test.SUBTYPE_MANUAL_TEST, new MyWorkItemsSublist(Test.SUBTYPE_MANUAL_TEST) },
                    { TestGherkin.SUBTYPE_GHERKIN_TEST, new MyWorkItemsSublist(TestGherkin.SUBTYPE_GHERKIN_TEST) },
                    { RunSuite.SUBTYPE_RUN_SUITE, new MyWorkItemsSublist(RunSuite.SUBTYPE_RUN_SUITE) },
                    { RunManual.SUBTYPE_RUN_MANUAL, new MyWorkItemsSublist(RunManual.SUBTYPE_RUN_MANUAL) },
                    { "comment", new MyWorkItemsSublist( "comment" )}
                };
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        #endregion
    }

    public class MyWorkItemsSublist
    {
        private string entityType { get; set; }
        public bool IsSelected { get; set; } = true;
        public EntityTypeInformation TypeInformation { get; }
        public ObservableCollection<OctaneItemViewModel> Items { get; set; }

        public MyWorkItemsSublist(string entityType)
        {
            TypeInformation = EntityTypeRegistry.GetEntityTypeInformation(entityType);
            Items = new ObservableCollection<OctaneItemViewModel>();
        }

    }

}
