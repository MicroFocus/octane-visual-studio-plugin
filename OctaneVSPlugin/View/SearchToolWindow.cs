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
using Microsoft.VisualStudio.Shell;
using System.Runtime.InteropServices;

namespace MicroFocus.Adm.Octane.VisualStudio.View
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("a01ef1da-eec6-4128-a474-7dd4f2d4c72d")]
    public class SearchToolWindow : ToolWindowPane
    {
        private readonly SearchToolWindowControl _searchControl;
        /// <summary>
        /// Initializes a new instance of the <see cref="SearchToolWindow"/> class.
        /// </summary>
        public SearchToolWindow() : base(null)
        {
            Caption = "SearchToolWindow";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            _searchControl = new SearchToolWindowControl();
            Content = _searchControl;
        }

        internal void Search(string searchFilter)
        {
            var viewModel = new SearchItemsViewModel();
            viewModel.Search(searchFilter);
            _searchControl.DataContext = viewModel;
        }
    }
}
