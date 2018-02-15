//------------------------------------------------------------------------------
// <copyright file="MainWindowControl.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Hpe.Nga.Octane.VisualStudio.Common;
using MicroFocus.Adm.Octane.Api.Core.Entities;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using octane_visual_studio_plugin;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Hpe.Nga.Octane.VisualStudio
{
    /// <summary>
    /// Interaction logic for MainWindowControl.
    /// </summary>
    public partial class MainWindowControl : UserControl
    {
        private readonly OctaneMyItemsViewModel viewModel;
        private MainWindowPackage package;

        private readonly MenuItem viewDetailsMenuItem;
        private readonly MenuItem openInBrowserMenuItem;
        private readonly MenuItem copyCommitMessageMenuItem;
        private readonly MenuItem gherkinTestMenuItem;

        private const string AppName = "ALM Octane";

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowControl"/> class.
        /// </summary>
        public MainWindowControl()
        {
            InitializeComponent();
            viewModel = new OctaneMyItemsViewModel();
            DataContext = viewModel;

            viewDetailsMenuItem = new MenuItem
            {
                Header = "View details (DblClick)",
                Command = new DelegatedCommand(ViewDetails)
            };

            openInBrowserMenuItem = new MenuItem
            {
                Header = "Open in Browser (Alt + DblClick)",
                Command = new DelegatedCommand(OpenInBrowser)
            };

            copyCommitMessageMenuItem = new MenuItem
            {
                Header = "Copy Commit Message to Clipboard (Shift+Alt+DblClick)",
                Command = new DelegatedCommand(CopyCommitMessage)
            };

            gherkinTestMenuItem = new MenuItem
            {
                Header = "Download Script",
                Command = new DelegatedCommand(DownloadGherkinScript)
            };
        }

        public void SetPackage(MainWindowPackage package)
        {
            this.package = package;
            viewModel.SetPackage(package);
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
            try
            {
                var selectedEntity = GetSelectedEntity();
                OpenInBrowserInternal(selectedEntity.Id, Utility.GetEntityType(selectedEntity));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to open item in browser.\n\n" + "Failed with message: " + ex.Message, AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenInBrowserInternal(EntityId id, string type)
        {
            var url = $"{package.AlmUrl}/ui/entity-navigation?p={package.SharedSpaceId}/{package.WorkSpaceId}&entityType={type}&id={id}";

            // Open the URL in the user's default browser.
            System.Diagnostics.Process.Start(url);
        }

        private async void ViewDetails(object param)
        {
            try
            {
                var selectedEntity = GetSelectedEntity();
                if (selectedEntity.TypeName == "feature" || selectedEntity.TypeName == "epic")
                {
                    OpenInBrowserInternal(selectedEntity.Id, "work_item");
                    return;
                }

                var entity = await viewModel.GetItem(selectedEntity);

                ToolWindowPane window = CreateDetailsWindow(entity);
                IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to open details window.\n\n" + "Failed with message: " + ex.Message, AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private ToolWindowPane CreateDetailsWindow(OctaneItemViewModel item)
        {
            // Create the window with the first free ID.   
            DetailsToolWindow toolWindow = (DetailsToolWindow)package.FindToolWindow(typeof(DetailsToolWindow), GetItemIDAsInt(item), true);
            if (toolWindow?.Frame == null)
            {
                throw new NotSupportedException("Cannot create tool window");
            }

            toolWindow.SetWorkItem(item);

            return toolWindow;
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

        private void CopyCommitMessage(object sender)
        {
            try
            {
                if (SelectedItem.IsSupportCopyCommitMessage)
                {
                    string message = SelectedItem.CommitMessage;
                    Clipboard.SetText(message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to obtain commit message.\n\n" + "Failed with message: " + ex.Message, AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void results_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (SelectedItem == null)
                return;

            if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
            {
                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    CopyCommitMessage(sender);
                }
                else
                {
                    OpenInBrowser(sender);
                }
            }
            else
            {
                ViewDetails(sender);
            }
        }

        private async void DownloadGherkinScript(object sender)
        {
            try
            {
                Test test = (Test)SelectedItem.Entity;
                string script = await viewModel.GetGherkinScript(test);

                package.CreateFile(test.Name, script);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to obtain gherkin script.\n\n" + "Failed with message: " + ex.Message, AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ListMenu_Opened(object sender, RoutedEventArgs e)
        {
            var cm = (ContextMenu)sender;

            cm.Items.Clear();

            viewDetailsMenuItem.Header = !(SelectedItem is CommentViewModel)
                ? "View details (DblClick)"
                : "View parent details (DblClick)";
            cm.Items.Add(viewDetailsMenuItem);

            cm.Items.Add(openInBrowserMenuItem);

            if (SelectedItem.IsSupportCopyCommitMessage)
            {
                cm.Items.Add(copyCommitMessageMenuItem);
            }

            if (SelectedItem.SubType == "gherkin_test")
            {
                cm.Items.Add(gherkinTestMenuItem);
            }
        }

        private BaseEntity GetSelectedEntity()
        {
            var selectedEntity = SelectedItem.Entity;
            if (SelectedItem is CommentViewModel commentViewModel)
            {
                selectedEntity = commentViewModel.ParentEntity;
            }

            return selectedEntity;
        }
    }
}