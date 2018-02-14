//------------------------------------------------------------------------------
// <copyright file="MainWindowControl.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

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

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowControl"/> class.
        /// </summary>
        public MainWindowControl()
        {
            this.InitializeComponent();
            viewModel = new OctaneMyItemsViewModel();
            this.DataContext = viewModel;

            viewDetailsMenuItem = new MenuItem
            {
                Header = "View details (DblClick)",
                Command = new DelegatedCommand(ViewDetails)
            };

            openInBrowserMenuItem = new MenuItem
            {
                Header = "Open in Browser(Alt + DblClick)",
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

        #region OpenInBrowser

        private void OpenInBrowser(object param)
        {
            OpenInBrowser(SelectedItem.Entity.Id, SelectedItem.Entity.TypeName);
        }

        private void OpenInBrowser(EntityId id, string type)
        {
            string url = string.Format("{0}/ui/entity-navigation?p={1}/{2}&entityType={3}&id={4}",
                package.AlmUrl,
                package.SharedSpaceId,
                package.WorkSpaceId,
                type,
                id);

            try
            {
                // Open the URL in the user's default browser.
                System.Diagnostics.Process.Start(url);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fail to open the browser\n\n" + ex.Message, "Octane ALM", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        private async void ViewDetails(object param)
        {
            var selectedEntity = SelectedItem.Entity;
            if (SelectedItem is CommentViewModel commentViewModel)
            {
                selectedEntity = commentViewModel.ParentEntity;
            }

            if (selectedEntity.TypeName == "feature" || selectedEntity.TypeName == "epic")
            {
                OpenInBrowser(selectedEntity.Id, "work_item");
                return;
            }

            try
            {
                var entity = await viewModel.GetItem(selectedEntity);

                ToolWindowPane window = CreateDetailsWindow(entity);
                IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
                Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fail to open details window\n\n" + ex.Message, "Octane ALM", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private ToolWindowPane CreateDetailsWindow(OctaneItemViewModel item)
        {
            // Create the window with the first free ID.   
            DetailsToolWindow toolWindow = (DetailsToolWindow)this.package.FindToolWindow(typeof(DetailsToolWindow), GetItemIDAsInt(item), true);

            if ((null == toolWindow) || (null == toolWindow.Frame))
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
            if (SelectedItem.IsSupportCopyCommitMessage)
            {
                string message = SelectedItem.CommitMessage;
                Clipboard.SetText(message);
            }
        }

        private void results_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
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
            catch
            {
                MessageBox.Show("Fail to get test script");
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
    }
}