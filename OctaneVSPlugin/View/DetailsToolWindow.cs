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
using MicroFocus.Adm.Octane.VisualStudio.ViewModel;
using Microsoft.VisualStudio.Shell;
using System.Collections.Generic;
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
    [Guid("d19915c9-3dea-4d5c-aa56-bd1fed3a7ab3")]
    public class DetailsToolWindow : ToolWindowPane
    {
        private readonly OctaneToolWindowControl detailsControl;

        private static readonly HashSet<string> supportedEntityTypes = new HashSet<string>
        {
            // work item
            WorkItem.SUBTYPE_DEFECT,
            WorkItem.SUBTYPE_STORY,
            WorkItem.SUBTYPE_QUALITY_STORY,

            // task
            "task",

            // test
            TestGherkin.SUBTYPE_GHERKIN_TEST,
            Test.SUBTYPE_MANUAL_TEST,

            // run
            RunManual.SUBTYPE_RUN_MANUAL,
            RunSuite.SUBTYPE_RUN_SUITE,

            // requirement
            Requirement.SUBTYPE_DOCUMENT
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="DetailsToolWindow"/> class.
        /// </summary>
        public DetailsToolWindow() : base(null)
        {
            Caption = "Loading entity...";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            detailsControl = new OctaneToolWindowControl();
            this.Content = detailsControl;
        }

        /// <inheritdoc/>
        protected override void OnClose()
        {
            DetailsWindowManager.UnregisterDetailsWindow(this);
            base.OnClose();
        }

        /// <summary>
        /// Load the necessary information for the given entity
        /// </summary>
        internal void LoadEntity(BaseEntity entity)
        {
            var metadata = new MyWorkMetadata();
            var viewModel = new DetailedItemViewModel(entity, metadata);
            viewModel.Initialize();
            Caption = $"{metadata.GetIconText(entity)} {viewModel.ID}";
            detailsControl.DataContext = viewModel;
        }

        /// <summary>
        /// Returns whether the given type can be shown by the details window.
        /// </summary>
        public static bool IsEntityTypeSupported(string type)
        {
            return supportedEntityTypes.Contains(type);
        }
    }
}
