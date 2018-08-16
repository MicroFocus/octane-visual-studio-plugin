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
using MicroFocus.Adm.Octane.VisualStudio.View;
using MicroFocus.Adm.Octane.VisualStudio.ViewModel;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xceed.Wpf.Toolkit;

namespace MicroFocus.Adm.Octane.VisualStudio
{
    /// <summary>
    /// Interaction logic for MainWindowControl.
    /// </summary>
    [ExcludeFromCodeCoverage]
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
        }

        /// <summary>
        /// Initialize the data for the control
        /// </summary>
        internal void Initialize()
        {
            _viewModel.LoadMyItemsAsync();
        }

        private OctaneItemViewModel SelectedItem
        {
            get
            {
                return (OctaneItemViewModel)results.SelectedItem;
            }
        }

        private void OpenInBrowser(object param)
        {
            ToolWindowHelper.OpenInBrowser(GetSelectedEntity());
        }

        private void ViewDetails(object param)
        {
            ToolWindowHelper.ViewDetails(GetSelectedEntity());
        }

        private void ViewTaskParentDetails(object param)
        {
            ToolWindowHelper.ViewTaskParentDetails(SelectedItem);
        }

        private void ViewCommentParentDetails(object param)
        {
            ToolWindowHelper.ViewCommentParentDetails(SelectedItem);
        }

        private void CopyCommitMessage(object sender)
        {
            try
            {
                SelectedItem.ValidateCommitMessage();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Unable to obtain commit message.\n\n" + "Failed with message: " + ex.Message, ToolWindowHelper.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void results_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ToolWindowHelper.HandleDoubleClickOnItem(GetSelectedEntity(), CopyCommitMessage);
        }

        private void DownloadGherkinScript(object sender)
        {
            ToolWindowHelper.DownloadGherkinScript(SelectedItem);
        }

        private void StartWork(object sender)
        {
            try
            {
                if (SelectedItem?.Entity == null)
                    return;

                OctaneItemViewModel.SetActiveItem(SelectedItem);

                MainWindowCommand.Instance.UpdateActiveItemInToolbar();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Unable to start work on current item.\n\n" + "Failed with message: " + ex.Message, ToolWindowHelper.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StopWork(object sender)
        {
            try
            {
                if (SelectedItem?.Entity == null)
                    return;

                OctaneItemViewModel.ClearActiveItem();

                MainWindowCommand.Instance.UpdateActiveItemInToolbar();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Unable to stop work on current item.\n\n" + "Failed with message: " + ex.Message, ToolWindowHelper.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ListMenu_Opened(object sender, RoutedEventArgs e)
        {
            ToolWindowHelper.ConstructContextMenu(sender as ContextMenu, SelectedItem,
                ViewDetails, ViewTaskParentDetails, ViewCommentParentDetails,
                OpenInBrowser, CopyCommitMessage, DownloadGherkinScript,
                StartWork, StopWork);
        }

        private BaseEntity GetSelectedEntity()
        {
            var selectedEntity = SelectedItem?.Entity;
            if (SelectedItem is CommentViewModel commentViewModel)
            {
                selectedEntity = commentViewModel.ParentEntity;
            }

            return selectedEntity;
        }
    }
}