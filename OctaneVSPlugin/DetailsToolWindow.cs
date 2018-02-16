//------------------------------------------------------------------------------
// <copyright file="DetailsToolWindow.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace MicroFocus.Adm.Octane.VisualStudio
{
    using Microsoft.VisualStudio.Shell;
    using System;
    using System.Runtime.InteropServices;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="DetailsToolWindow"/> class.
        /// </summary>
        public DetailsToolWindow() : base(null)
        {
            this.Caption = "OctaneToolWindow";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            detailsControl = new OctaneToolWindowControl();
            this.Content = detailsControl;
        }

        internal void SetWorkItem(OctaneItemViewModel itemViewModel)
        {
            this.Caption = string.Format("Item #{0}", itemViewModel.ID);
            detailsControl.DataContext = itemViewModel;
        }
    }
}
