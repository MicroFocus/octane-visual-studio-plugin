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

namespace MicroFocus.Adm.Octane.VisualStudio
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for OctaneToolWindowControl.
    /// </summary>
    public partial class OctaneToolWindowControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OctaneToolWindowControl"/> class.
        /// </summary>
        public OctaneToolWindowControl()
        {
            this.InitializeComponent();
            DataContextChanged += OctaneToolWindowControl_DataContextChanged;
        }

        private void OctaneToolWindowControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var itemViewModel = DataContext as OctaneItemViewModel;
            if (itemViewModel != null)
            {
                string octaneImageBaseUrl = string.Format("{0}/api/shared_spaces/{1}/workspaces/{2}/attachments/",
                    OctaneMyItemsViewModel.Instance.Package.AlmUrl,
                    OctaneMyItemsViewModel.Instance.Package.SharedSpaceId,
                    OctaneMyItemsViewModel.Instance.Package.WorkSpaceId);

                string htmlWithImageUrls = itemViewModel.Description.Replace("file://[IMAGE_BASE_PATH_PLACEHOLDER]", octaneImageBaseUrl);

                if (!string.IsNullOrWhiteSpace(htmlWithImageUrls))
                {
                    DescBrowser.NavigateToString(htmlWithImageUrls);
                }
            }
        }

        private void DescBrowser_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (e.Uri != null)
            {
                e.Cancel = true;
                System.Diagnostics.Process.Start(e.Uri.ToString());
            }
        }
    }
}