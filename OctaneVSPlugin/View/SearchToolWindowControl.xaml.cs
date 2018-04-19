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

using MicroFocus.Adm.Octane.VisualStudio.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace MicroFocus.Adm.Octane.VisualStudio.View
{
    /// <summary>
    /// Interaction logic for SearchToolWindowControl.
    /// </summary>
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

        private void OpenInBrowser(object param)
        {
            ToolWindowHelper.OpenInBrowser(SelectedItem?.Entity);
        }

        private void DownloadGherkinScript(object sender)
        {
            ToolWindowHelper.DownloadGherkinScript(SelectedItem);
        }

        private void SearchResults_ContextMenuOpened(object sender, RoutedEventArgs e)
        {
            ToolWindowHelper.ConstructContextMenu(sender as ContextMenu, SelectedItem,
                null, null, null,
                OpenInBrowser, null, DownloadGherkinScript);
        }
    }
}