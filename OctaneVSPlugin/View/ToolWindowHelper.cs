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
using MicroFocus.Adm.Octane.VisualStudio.Common;
using MicroFocus.Adm.Octane.VisualStudio.ViewModel;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MicroFocus.Adm.Octane.VisualStudio.View
{
    /// <summary>
    /// Helper methods for ToolWindow commands/operations
    /// </summary>
    internal static class ToolWindowHelper
    {
        internal const string AppName = "ALM Octane";

        internal const string ViewDetailsHeader = "View details (DblClick)";
        internal const string ViewTaskParentDetailsHeader = "View parent details (DblClick)";
        internal const string ViewCommentParentDetailsHeader = "View parent details (DblClick)";
        internal const string OpenInBrowserHeader = "Open in Browser (Alt + DblClick)";
        internal const string CopyCommitMessageHeader = "Copy Commit Message to Clipboard (Shift+Alt+DblClick)";
        internal const string DownloadGherkinScriptHeader = "Download Script";
        internal const string StartWorkHeader = "Start Work";

        /// <summary>
        /// Handle double-click event on a backlog item
        /// </summary>
        [ExcludeFromCodeCoverage]
        public static void HandleDoubleClickOnItem(BaseEntity entity, Action<object> copyCommitMessageDelegate = null)
        {
            try
            {
                if (entity == null)
                    return;

                if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
                {
                    if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                    {
                        copyCommitMessageDelegate?.Invoke(null);
                    }
                    else
                    {
                        OpenInBrowser(entity);
                    }
                }
                else
                {
                    if (DetailsToolWindow.IsEntityTypeSupported(Utility.GetConcreteEntityType(entity)))
                    {
                        ViewDetails(entity);
                    }
                    else
                    {
                        OpenInBrowser(entity);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to process double click operation.\n\n" + "Failed with message: " + ex.Message, ToolWindowHelper.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// View given entity's details in a new window
        /// </summary>
        [ExcludeFromCodeCoverage]
        public static void ViewDetails(BaseEntity entity)
        {
            try
            {
                if (entity == null)
                    return;

                ViewEntityDetailsInternal(entity);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to open details window.\n\n" + "Failed with message: " + ex.Message, ToolWindowHelper.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// View given item's parent details in a new window
        /// </summary>
        [ExcludeFromCodeCoverage]
        public static void ViewTaskParentDetails(BaseItemViewModel selectedItem)
        {
            try
            {
                if (selectedItem?.Entity == null)
                    return;

                if (selectedItem.Entity.TypeName != Task.TYPE_TASK)
                {
                    throw new Exception($"Unrecognized type {selectedItem.Entity.TypeName}.");
                }
                var selectedEntity = (BaseEntity)selectedItem.Entity.GetValue("story");

                ViewEntityDetailsInternal(selectedEntity);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to open details window.\n\n" + "Failed with message: " + ex.Message, ToolWindowHelper.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// View given item's parent details in a new window
        /// </summary>
        [ExcludeFromCodeCoverage]
        public static void ViewCommentParentDetails(BaseItemViewModel selectedItem)
        {
            try
            {
                var commentViewModel = selectedItem as CommentViewModel;
                if (commentViewModel == null)
                {
                    throw new Exception($"Unrecognized type {selectedItem.Entity.TypeName}.");
                }
                var selectedEntity = commentViewModel.ParentEntity;

                ViewEntityDetailsInternal(selectedEntity);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to open details window.\n\n" + "Failed with message: " + ex.Message, ToolWindowHelper.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [ExcludeFromCodeCoverage]
        private static void ViewEntityDetailsInternal(BaseEntity entity)
        {
            if (entity.TypeName == WorkItem.SUBTYPE_FEATURE || entity.TypeName == WorkItem.SUBTYPE_EPIC)
            {
                Utility.OpenInBrowser(entity);
                return;
            }

            PluginWindowManager.ShowDetailsWindow(MainWindow.PluginPackage, entity);
        }

        /// <summary>
        /// Open the given entity in the browser
        /// </summary>
        [ExcludeFromCodeCoverage]
        internal static void OpenInBrowser(BaseEntity entity)
        {
            try
            {
                if (entity == null)
                    return;

                Utility.OpenInBrowser(entity);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to open item in browser.\n\n" + "Failed with message: " + ex.Message, AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Download the gherkin script for the selected item if possible
        /// </summary>
        internal static async void DownloadGherkinScript(BaseItemViewModel selectedItem)
        {
            try
            {
                if (selectedItem?.Entity == null)
                    return;

                var test = selectedItem.Entity as Test;
                if (test == null)
                    return;

                OctaneServices octane = new OctaneServices(
                    OctaneConfiguration.Url,
                    OctaneConfiguration.SharedSpaceId,
                    OctaneConfiguration.WorkSpaceId,
                    OctaneConfiguration.Username,
                    OctaneConfiguration.Password);
                await octane.Connect();

                var testScript = await octane.GetTestScript(test.Id);
                MainWindow.PluginPackage.CreateFile(test.Name, testScript.Script);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to obtain gherkin script.\n\n" + "Failed with message: " + ex.Message, ToolWindowHelper.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Construct the context menu for the given selected item
        /// </summary>
        internal static void ConstructContextMenu(ContextMenu cm, BaseItemViewModel selectedItem,
            Action<object> viewDetailsDelegate,
            Action<object> viewTaskParentDetailsDelegate,
            Action<object> viewCommentParentDetailsDelegate,
            Action<object> openInBrowserDelegate,
            Action<object> copyCommitMessageDelegate,
            Action<object> downloadGherkinScriptDelegate,
            Action<object> startWorkDelegate)
        {
            try
            {
                if (cm == null)
                    return;

                cm.Items.Clear();

                if (selectedItem == null)
                    return;

                var selectedEntity = selectedItem.Entity;

                var entityType = Utility.GetConcreteEntityType(selectedEntity);

                // view details
                if (viewDetailsDelegate != null
                    && DetailsToolWindow.IsEntityTypeSupported(entityType))
                {
                    cm.Items.Add(new MenuItem
                    {
                        Header = ViewDetailsHeader,
                        Command = new DelegatedCommand(viewDetailsDelegate)
                    });
                }

                // view task parent details
                var taskParentEntity = Utility.GetTaskParentEntity(selectedEntity);
                if (viewTaskParentDetailsDelegate != null
                    && selectedEntity.TypeName == Task.TYPE_TASK
                    && taskParentEntity != null
                    && DetailsToolWindow.IsEntityTypeSupported(entityType))
                {
                    cm.Items.Add(new MenuItem
                    {
                        Header = ViewTaskParentDetailsHeader,
                        Command = new DelegatedCommand(viewTaskParentDetailsDelegate)
                    });
                }

                // view comment parent details
                var commentParentEntity = GetCommentParentEntity(selectedItem);
                if (viewCommentParentDetailsDelegate != null
                    && commentParentEntity != null
                    && DetailsToolWindow.IsEntityTypeSupported(entityType))
                {
                    cm.Items.Add(new MenuItem
                    {
                        Header = ViewCommentParentDetailsHeader,
                        Command = new DelegatedCommand(viewCommentParentDetailsDelegate)
                    });
                }

                // open in browser
                if (openInBrowserDelegate != null)
                {
                    cm.Items.Add(new MenuItem
                    {
                        Header = OpenInBrowserHeader,
                        Command = new DelegatedCommand(openInBrowserDelegate)
                    });
                }

                // coppy commit message
                if (copyCommitMessageDelegate != null)
                {
                    var octaneItem = selectedItem as OctaneItemViewModel;
                    if (octaneItem != null && octaneItem.IsSupportCopyCommitMessage)
                    {
                        cm.Items.Add(new MenuItem
                        {
                            Header = CopyCommitMessageHeader,
                            Command = new DelegatedCommand(copyCommitMessageDelegate)
                        });
                    }
                }

                // download gherkin script
                if (downloadGherkinScriptDelegate != null
                    && entityType == TestGherkin.SUBTYPE_GHERKIN_TEST)
                {
                    cm.Items.Add(new MenuItem
                    {
                        Header = DownloadGherkinScriptHeader,
                        Command = new DelegatedCommand(downloadGherkinScriptDelegate)
                    });
                }

                if (startWorkDelegate != null
                    && (entityType == WorkItem.SUBTYPE_STORY
                        || entityType == WorkItem.SUBTYPE_QUALITY_STORY
                        || entityType == WorkItem.SUBTYPE_DEFECT
                        || entityType == Task.TYPE_TASK))
                {
                    cm.Items.Add(new MenuItem
                    {
                        Header = StartWorkHeader,
                        Command = new DelegatedCommand(startWorkDelegate)
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to show context menu.\n\n" + "Failed with message: " + ex.Message, AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static BaseEntity GetCommentParentEntity(BaseItemViewModel selectedItem)
        {
            if (selectedItem is CommentViewModel commentViewModel)
            {
                return commentViewModel.ParentEntity;
            }

            return null;
        }
    }
}
