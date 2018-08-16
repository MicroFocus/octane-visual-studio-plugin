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
    using Microsoft.VisualStudio.Shell;
    using octane_visual_studio_plugin;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.InteropServices;
    using Xceed.Wpf.Toolkit;

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
    [Guid(WINDOW_ID)]
    [ExcludeFromCodeCoverage]
    public class MainWindow : ToolWindowPane
    {
        internal const string WINDOW_ID = "af5c5224-1b4a-444f-923f-2fc9e06f7a40";

        private readonly MainWindowControl _mainWindowControl;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow() : base(null)
        {
            Caption = "ALM Octane - My Work";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            _mainWindowControl = new MainWindowControl();
            Content = _mainWindowControl;
        }

        /// <summary>
        /// Reference to the package
        /// </summary>
        internal static MainWindowPackage PluginPackage { get; private set; }

        /// <inheritdoc/>>
        protected override void OnCreate()
        {
            base.OnCreate();
            PluginPackage = (MainWindowPackage)Package;
            _mainWindowControl.Initialize();
        }
    }
}
