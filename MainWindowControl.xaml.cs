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
    using System.Windows.Input;

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

        OctaneItemViewModel SelectedItem
        {
            get
            {
                return (OctaneItemViewModel)results.SelectedItem;
            }
        }
        
        private void OpenInBrowser_Click(object sender, RoutedEventArgs e)
        {
            // url: http://myd-vm10629.hpeswlab.net:8081
            // http://myd-vm10629.hpeswlab.net:8081/ui/entity-navigation?p=1001/1002&entityType=work_item&id=1111
            var sb = new StringBuilder();
            var selectedId = SelectedItem.ID;
            sb.AppendFormat("{0}/ui/entity-navigation?p={1}/{2}&entityType=work_item&id={3}", package.AlmUrl, package.SharedSpaceId, package.WorkSpaceId, selectedId);
            System.Diagnostics.Process.Start(sb.ToString());
        }

       

        private void ShowDetails_Click(object sender, RoutedEventArgs e)
        {
            ToolWindowPane window = CreateDetailsWindow(SelectedItem);
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());

        }

        private ToolWindowPane CreateDetailsWindow(OctaneItemViewModel item)
        {
            // Create the window with the first free ID.   
            DetailsToolWindow toolWindow = (DetailsToolWindow)this.package.FindToolWindow(typeof(DetailsToolWindow), GetItemIDAsInt(item), true);

            if ((null == toolWindow) || (null == toolWindow.Frame))
            {
                throw new NotSupportedException("Cannot create tool window");
            }

            toolWindow.SetWorkItem(SelectedItem);

            return toolWindow;
        }

        private ToolWindowPane GetDetailsWindow(OctaneItemViewModel item)
        {
            return this.package.FindToolWindow(typeof(DetailsToolWindow), GetItemIDAsInt(item), false);
        }

        /// <summary>
        /// Octane treat WorkItem ID as long (64 bit) and Visual Studio needs int (32 bit) to identify tool windows.
        /// This function safely convert long to int.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private int GetItemIDAsInt(OctaneItemViewModel item)
        {
            return item.GetHashCode();
        }

        private void GenerateCommitMsg_Click(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            var item = SelectedItem;
            sb.AppendFormat("{0} [{1}]: {2}", item.ID, item.Phase, item.Name);
            Clipboard.SetText(sb.ToString());
        }

        private void results_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
            {
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    GenerateCommitMsg_Click(sender, e);
                }
                else
                {
                    OpenInBrowser_Click(sender, e);
                }
            }
            else
            {
                ShowDetails_Click(sender, e);
            }
                
        }
    }
}