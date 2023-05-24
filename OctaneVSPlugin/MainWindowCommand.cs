/*!
* Copyright 2017 – 2023 Open Text.
*
* The only warranties for products and services of Open Text and
* its affiliates and licensors (“Open Text”) are as may be set
* forth in the express warranty statements accompanying such products
* and services. Nothing herein should be construed as constituting an
* additional warranty. Open Text shall not be liable for technical or
* editorial errors or omissions contained herein. The information
* contained herein is subject to change without notice.
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

using MicroFocus.Adm.Octane.VisualStudio.Common;
using MicroFocus.Adm.Octane.VisualStudio.View;
using MicroFocus.Adm.Octane.VisualStudio.ViewModel;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace MicroFocus.Adm.Octane.VisualStudio
{
    /// <summary>
    /// Command handler
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal sealed class MainWindowCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        public const int ShowActiveItemCommandId = 0x0400;
        public const int CopyCommitMessageCommandId = 0x0401;
        public const int StopWorkCommandId = 0x402;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("085d7d5b-5b56-4870-80b4-0ca77f9cf140");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package _package;

        private readonly OleMenuCommand _activeItemMenuCommand;
        private readonly OleMenuCommand _copyCommitMessageCommand;
        private readonly OleMenuCommand _stopWorkCommand;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private MainWindowCommand(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            _package = package;

            var commandService = ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                // register show tool window command
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(ShowToolWindow, menuCommandID);
                commandService.AddCommand(menuItem);

                // register active item command
                menuCommandID = new CommandID(CommandSet, ShowActiveItemCommandId);
                _activeItemMenuCommand = new OleMenuCommand(OpenActiveItemInDetailsWindowCallback, menuCommandID);
                commandService.AddCommand(_activeItemMenuCommand);

                // register copy commit message command
                menuCommandID = new CommandID(CommandSet, CopyCommitMessageCommandId);
                _copyCommitMessageCommand = new OleMenuCommand(CopyCommitMessageCallback, menuCommandID);
                commandService.AddCommand(_copyCommitMessageCommand);

                // register stop work command
                menuCommandID = new CommandID(CommandSet, StopWorkCommandId);
                _stopWorkCommand = new OleMenuCommand(StopWorkCallback, menuCommandID);
                commandService.AddCommand(_stopWorkCommand);
                

                DisableActiveItemToolbar();
            }
        }

        private static void OpenActiveItemInDetailsWindowCallback(object caller, EventArgs args)
        {
            try
            {
                var command = caller as OleMenuCommand;
                if (command == null)
                    return;

                var activeEntity = WorkspaceSessionPersistanceManager.GetActiveEntity();
                if (activeEntity == null)
                    return;

                PluginWindowManager.ShowDetailsWindow(MainWindow.PluginPackage, activeEntity);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to open details window for active item.\n\n" + "Failed with message: " + ex.Message,
                    ToolWindowHelper.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static async void CopyCommitMessageCallback(object caller, EventArgs args)
        {
            try
            {
                var command = caller as OleMenuCommand;
                if (command == null)
                    return;

                if (OctaneItemViewModel.CurrentActiveItem == null)
                    return;

                if (!OctaneItemViewModel.CurrentActiveItem.IsSupportCopyCommitMessage)
                    return;

                await OctaneItemViewModel.CurrentActiveItem.ValidateCommitMessage();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to generate and copy commit message to clipboard.\n\n" + "Failed with message: " + ex.Message,
                    ToolWindowHelper.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void StopWorkCallback(object caller, EventArgs args)
        {
            try
            {
                var command = caller as OleMenuCommand;
                if (command == null)
                    return;
                if (OctaneItemViewModel.CurrentActiveItem == null)
                    return;

                    
                OctaneItemViewModel.ClearActiveItem();
                Instance.UpdateActiveItemInToolbar();

            } catch (Exception ex)
            {
                MessageBox.Show("Unable to stop work on current item.\n\n" + "Failed with message: " + ex.Message,
                    ToolWindowHelper.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }


        }

        /// <summary>
        /// Update active item button in toolbar with the current active item's information
        /// </summary>
        public void UpdateActiveItemInToolbar()
        {
            try
            {
                var activeEntity = WorkspaceSessionPersistanceManager.GetActiveEntity();

                if (activeEntity != null)
                {   
                    _activeItemMenuCommand.Text = EntityTypeRegistry.GetEntityTypeInformation(activeEntity).ShortLabel + " " + activeEntity.Id;
                    _activeItemMenuCommand.Enabled = true;
                    _copyCommitMessageCommand.Enabled = true;
                    _stopWorkCommand.Enabled = true;
                }
                else
                {
                    DisableActiveItemToolbar();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to update active item in Octane toolbar.\n\n" + "Failed with message: " + ex.Message,
                    ToolWindowHelper.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Disable active item toolbar
        /// </summary>
        public void DisableActiveItemToolbar()
        {
            try
            {
                _activeItemMenuCommand.Text = "N/A";
                _activeItemMenuCommand.Enabled = false;
                _copyCommitMessageCommand.Enabled = false;
                _stopWorkCommand.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to disable active item in Octane toolbar.\n\n" + "Failed with message: " + ex.Message,
                    ToolWindowHelper.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static MainWindowCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return _package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new MainWindowCommand(package);
        }

        /// <summary>
        /// Shows the tool window when the menu item is clicked.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        private void ShowToolWindow(object sender, EventArgs e)
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = _package.FindToolWindow(typeof(MainWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException("Cannot create tool window");
            }

            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }
    }
}
