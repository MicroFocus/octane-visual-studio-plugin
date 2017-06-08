//------------------------------------------------------------------------------
// <copyright file="MainWindowControl.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Hpe.Nga.Octane.VisualStudio
{
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;
    using Hpe.Nga.Api.Core.Connector;
    using Hpe.Nga.Api.Core.Entities;
    using Hpe.Nga.Api.Core.Services;
    using System;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using octane_visual_studio_plugin;
    using System.Text;

    /// <summary>
    /// Interaction logic for MainWindowControl.
    /// </summary>
    public partial class MainWindowControl : UserControl
    {
        private readonly OctaneMyItemsViewModel viewModel;
        private MainWindowPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowControl"/> class.
        /// </summary>
        public MainWindowControl()
        {
            this.InitializeComponent();
            viewModel = new OctaneMyItemsViewModel();
            this.DataContext = viewModel;
        }

        public void SetPackage(MainWindowPackage package)
        {
            this.package = package;
            viewModel.SetPackage(package);
        }

        private void OpenInBrowser_Click(object sender, RoutedEventArgs e)
        {
            // url: http://myd-vm10629.hpeswlab.net:8081
            // http://myd-vm10629.hpeswlab.net:8081/ui/entity-navigation?p=1001/1002&entityType=work_item&id=1111
            var sb = new StringBuilder();
            var selectedId = ((OctaneItemViewModel)this.results.SelectedItem).ID;
            sb.AppendFormat("{0}/ui/entity-navigation?p={1}/{2}&entityType=work_item&id={3}", package.AlmUrl, package.SharedSpaceId, package.WorkSpaceId, selectedId);
            System.Diagnostics.Process.Start(sb.ToString());
        }

        private void ShowDetails_Click(object sender, RoutedEventArgs e)
        {
            int selectedId = (int)((OctaneItemViewModel)results.SelectedItem).ID;
            ToolWindowPane window = this.package.FindToolWindow(typeof(OctaneToolWindow), selectedId, false);
            if (window == null)
            {
                // Create the window with the first free ID.   
                window = (ToolWindowPane)this.package.FindToolWindow(typeof(OctaneToolWindow), selectedId, true);
                if ((null == window) || (null == window.Frame))
                {
                    throw new NotSupportedException("Cannot create tool window");
                }
                window.Caption = "Item " + selectedId;
            }
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());

        }

        private void ToggleActive_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}