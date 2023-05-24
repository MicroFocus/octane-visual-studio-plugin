/*!
* Copyright 2017 – 2023 Open Text.
*
* The only warranties for products and services of Open Text and
* its affiliates and licensors (“Open Text”) are as may be set
* forth in the express warranty statements accompanying such products
* and services. Nothing herein should be construed as constituting an
* additional warranty. Open Text shall not be liable for technical or
* editorial errors or omissions contained herein. The information
* contained herein is subject to change without notice.
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


using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MicroFocus.Adm.Octane.Api.Core.Entities;
using MicroFocus.Adm.Octane.Api.Core.Services;
using MicroFocus.Adm.Octane.VisualStudio.ViewModel;
using System.ComponentModel;
using System.Timers;
using System;

namespace MicroFocus.Adm.Octane.VisualStudio.View
{
    /// <summary>
    /// Interaction logic for EntityComboBox.xaml
    /// </summary>
    public partial class EntityComboBox : UserControl, INotifyPropertyChanged
    {
        public Visibility HasSearchBox
        {
            get { return SearchBox.Visibility; }
            set { SearchBox.Visibility = value;}
        }

        private Visibility _MultiSelectVisibility;
        
        public Visibility MultiSelectVisibility {
            get { return _MultiSelectVisibility; }
            set { _MultiSelectVisibility = value; }
        }

        public WindowMode ComboBoxMode { get; set; }
        private string _filter = string.Empty;
        Timer delayTimer;

        public EntityComboBox()
        {
            InitializeComponent();
            InitializeTimer();
        }

        public ObservableCollection<BaseEntity> SelectedValues;

        private void InitializeTimer()
        {
            delayTimer = new Timer();
            delayTimer.Elapsed += new ElapsedEventHandler(delayTimer_Tick);
            delayTimer.Interval = 500;
        }

        private void SelectionHandler(object sender, MouseButtonEventArgs e)
        {
            var item = ((sender as ListViewItem).DataContext as BaseEntityWrapper);
            
            if (item != null)
            {
                if (MultiSelectVisibility == Visibility.Visible)
                {
                    if (item.IsSelected.Equals(true))
                    {
                        //take the selected item out of the selected items list
                        EntityList<BaseEntity> selectedEntities = ((FieldViewModel)DataContext).GetSelectedEntities();
                        
                        BaseEntity entityToRemove = null;
                        foreach (BaseEntity baseEntity in selectedEntities.data)
                        {
                            if(item.Equals(new BaseEntityWrapper(baseEntity)))
                            {
                                entityToRemove = baseEntity;
                                break;
                            }
                        }
                        selectedEntities.data.Remove(entityToRemove);
                        ((FieldViewModel)EditorLabelName.DataContext).Content = selectedEntities;

                        item.IsSelected = false;
                    }
                    else
                    {
                        //insert the selected value into the selected items list
                        EntityList<BaseEntity> selectedEntities = ((FieldViewModel)DataContext).GetSelectedEntities();
                        
                        selectedEntities.data.Add(item.BaseEntity);
                        ((FieldViewModel)EditorLabelName.DataContext).Content = selectedEntities;

                        item.IsSelected = true;
                    }

                    // Set the focus back to the list view item, if the combo box was clicked directly the popup would no longer close, because the focus would go crazy
                    FocusManager.SetFocusedElement(this, (sender as ListViewItem));
                } else
                {
                    ComboBoxPopup.IsOpen = false;
                    ((FieldViewModel)EditorLabelName.DataContext).Content = item.BaseEntity;
                }
                listView.Items.Refresh();
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
                delayTimer.Stop();
                _filter = value?.ToLowerInvariant() ?? string.Empty;
                delayTimer.Start();
            }
        }

        private void delayTimer_Tick(object sender, ElapsedEventArgs e)
        {
            delayTimer.Stop();
            this.Dispatcher.Invoke(() => { LoadItems(); });
        }

        /// <summary>
        /// Load the list of the popup
        /// </summary>
        private void LoadItems()
        {
            ComboBoxMode = WindowMode.Loading;
            NotifyPropertyChanged("ComboBoxMode");

            var possibleItems = ((FieldViewModel)EditorLabelName.DataContext).getRelatedEntities(_filter);
            System.Threading.Tasks.Task taskRetrieveData = new System.Threading.Tasks.Task(async () =>
            {
                List<BaseEntityWrapper> results = await possibleItems;
                this.Dispatcher.Invoke(() =>
                {
                    listView.Items.Clear();
                    if (results != null)
                    {
                        results.ForEach(item => listView.Items.Add(item));
                    }
                    listView.Items.Refresh();

                    ComboBoxMode = WindowMode.Loaded;
                    NotifyPropertyChanged("ComboBoxMode");
                });
            });
            taskRetrieveData.Start();
        }

        /// <summary>
        /// Current status of the detailed view
        /// </summary>
        private void ShowPopup(object sender, RoutedEventArgs e)
        {
            LoadItems();
            
            ComboBoxPopup.IsOpen = true;
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private void NotifyPropertyChanged(string propName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        #endregion
    }
}
