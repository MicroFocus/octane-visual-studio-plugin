/*!
* © Copyright 2017-2022 Micro Focus or one of its affiliates.
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

using MicroFocus.Adm.Octane.VisualStudio.ViewModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;

namespace MicroFocus.Adm.Octane.VisualStudio.View
{
    /// <summary>
    /// Interaction logic for SearchToolWindowControl.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public partial class SearchToolWindowControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SearchToolWindowControl"/> class.
        /// </summary>
        public SearchToolWindowControl()
        {
            InitializeComponent();
        }

        private BaseItemViewModel SelectedItem
        {
            get { return (BaseItemViewModel)SearchResults.SelectedItem; }
        }

        private void SearchItems_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ToolWindowHelper.HandleDoubleClickOnItem(SelectedItem?.Entity);
        }

        private void ViewDetails(object param)
        {
            ToolWindowHelper.ViewDetails(SelectedItem?.Entity);
        }

        private void ViewTaskParentDetails(object param)
        {
            ToolWindowHelper.ViewTaskParentDetails(SelectedItem);
        }

        private void OpenInBrowser(object param)
        {
            ToolWindowHelper.OpenInBrowser(SelectedItem?.Entity);
        }

        private void DownloadScript(object sender)
        {
            ToolWindowHelper.DownloadScript(SelectedItem);
        }

        private void AddToMyWork(object sender)
        {
            ToolWindowHelper.AddToMyWork(SelectedItem?.Entity);
        }

        private void SearchResults_ContextMenuOpened(object sender, RoutedEventArgs e)
        {
            ToolWindowHelper.ConstructContextMenu(sender as ContextMenu, SelectedItem,
                ViewDetails, ViewTaskParentDetails, null,
                OpenInBrowser, null, DownloadScript, null, null, AddToMyWork, null);
        }
    }
}