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
using MicroFocus.Adm.Octane.VisualStudio.ViewModel;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MicroFocus.Adm.Octane.VisualStudio
{
    /// <summary>
    /// Interaction logic for MainWindowControl.
    /// </summary>
    public partial class MainWindowControl : UserControl
    {
        private readonly OctaneMyItemsViewModel _viewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowControl"/> class.
        /// </summary>
        public MainWindowControl()
        {
            InitializeComponent();
            _viewModel = new OctaneMyItemsViewModel();
            DataContext = _viewModel;

            SearchCommand = new DelegateCommand(SearchInternal);
        }

        /// <summary>
        /// Initialize the data for the control
        /// </summary>
        internal void Initialize()
        {
            _viewModel.LoadMyItems();
        }

        private OctaneItemViewModel SelectedItem
        {
            get
            {
                return (OctaneItemViewModel)results.SelectedItem;
            }
        }

        #region Search

        public string SearchFilter { get; set; }

        public ICommand SearchCommand { get; }

        private void SearchInternal(object parameter)
        {
            if (string.IsNullOrEmpty(SearchFilter))
                return;

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

        #endregion

        private void OpenInBrowser(object param)
        {
            ToolWindowHelper.OpenInBrowser(GetSelectedEntity());
        }

        private async void ViewDetails(object param)
        {
            try
            {
                var selectedEntity = GetSelectedEntity();
                await ViewEntityDetailsInternal(selectedEntity);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to open details window.\n\n" + "Failed with message: " + ex.Message, ToolWindowHelper.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ViewTaskParentDetails(object param)
        {
            try
            {
                if (SelectedItem.Entity.TypeName != Api.Core.Entities.Task.TYPE_TASK)
                {
                    throw new Exception($"Unrecognized type {SelectedItem.Entity.TypeName}.");
                }
                var selectedEntity = (BaseEntity)SelectedItem.Entity.GetValue("story");

                await ViewEntityDetailsInternal(selectedEntity);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to open details window.\n\n" + "Failed with message: " + ex.Message, ToolWindowHelper.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ViewCommentParentDetails(object param)
        {
            try
            {
                var commentViewModel = SelectedItem as CommentViewModel;
                if (commentViewModel == null)
                {
                    throw new Exception($"Unrecognized type {SelectedItem.Entity.TypeName}.");
                }
                var selectedEntity = commentViewModel.ParentEntity;

                await ViewEntityDetailsInternal(selectedEntity);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to open details window.\n\n" + "Failed with message: " + ex.Message, ToolWindowHelper.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task ViewEntityDetailsInternal(BaseEntity entity)
        {
            if (entity.TypeName == "feature" || entity.TypeName == "epic")
            {
                Utility.OpenInBrowser(entity);
                return;
            }

            DetailsToolWindow window = DetailsWindowManager.ObtainDetailsWindow(MainWindow.PluginPackage, entity);
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
            window.LoadEntity(entity);
        }

        private void CopyCommitMessage(object sender)
        {
            try
            {
                if (SelectedItem.IsSupportCopyCommitMessage)
                {
                    string message = SelectedItem.CommitMessage;
                    Clipboard.SetText(message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to obtain commit message.\n\n" + "Failed with message: " + ex.Message, ToolWindowHelper.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void results_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (SelectedItem == null)
                return;

            try
            {
                if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
                {
                    if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                    {
                        CopyCommitMessage(sender);
                    }
                    else
                    {
                        OpenInBrowser(sender);
                    }
                }
                else
                {
                    var selectedEntity = GetSelectedEntity();
                    if (DetailsToolWindow.IsEntityTypeSupported(Utility.GetConcreteEntityType(selectedEntity)))
                    {
                        ViewDetails(sender);
                    }
                    else
                    {
                        OpenInBrowser(sender);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to process double click operation.\n\n" + "Failed with message: " + ex.Message, ToolWindowHelper.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DownloadGherkinScript(object sender)
        {
            ToolWindowHelper.DownloadGherkinScript(SelectedItem);
        }

        private void ListMenu_Opened(object sender, RoutedEventArgs e)
        {
            ToolWindowHelper.ConstructContextMenu(sender as ContextMenu, SelectedItem,
                ViewDetails, ViewTaskParentDetails, ViewCommentParentDetails,
                OpenInBrowser, CopyCommitMessage, DownloadGherkinScript);
        }

        private BaseEntity GetSelectedEntity()
        {
            var selectedEntity = SelectedItem.Entity;
            if (SelectedItem is CommentViewModel commentViewModel)
            {
                selectedEntity = commentViewModel.ParentEntity;
            }

            return selectedEntity;
        }
    }
}