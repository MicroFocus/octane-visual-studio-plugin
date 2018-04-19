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
using System.Windows;
using System.Windows.Controls;

namespace MicroFocus.Adm.Octane.VisualStudio.View
{
    /// <summary>
    /// Helper methods for ToolWindow commands/operations
    /// </summary>
    internal static class ToolWindowHelper
    {
        internal const string AppName = "ALM Octane";

        /// <summary>
        /// Construct the context menu for the given selected item
        /// </summary>
        internal static void ConstructContextMenu(ContextMenu cm, BaseItemViewModel selectedItem,
            Action<object> viewDetailsDelegate,
            Action<object> viewTaskParentDetailsDelegate,
            Action<object> viewCommentParentDetailsDelegate,
            Action<object> openInBrowserDelegate,
            Action<object> copyCommitMessageDelegate,
            Action<object> downloadGherkinScriptDelegate)
        {
            try
            {
                if (cm == null)
                    return;

                cm.Items.Clear();

                if (selectedItem == null)
                    return;

                var selectedEntity = selectedItem.Entity;

                // view details
                if (viewDetailsDelegate != null
                    && DetailsToolWindow.IsEntityTypeSupported(Utility.GetConcreteEntityType(selectedEntity)))
                {
                    cm.Items.Add(new MenuItem
                    {
                        Header = "View details (DblClick)",
                        Command = new DelegatedCommand(viewDetailsDelegate)
                    });
                }

                // view task parent details
                var taskParentEntity = GetTaskParentEntity(selectedEntity);
                if (viewTaskParentDetailsDelegate != null
                    && selectedEntity.TypeName == Task.TYPE_TASK
                    && taskParentEntity != null
                    && DetailsToolWindow.IsEntityTypeSupported(Utility.GetConcreteEntityType(taskParentEntity)))
                {
                    cm.Items.Add(new MenuItem
                    {
                        Header = "View parent details (DblClick)",
                        Command = new DelegatedCommand(viewTaskParentDetailsDelegate)
                    });
                }

                // view comment parent details
                var commentParentEntity = GetCommentParentEntity(selectedItem);
                if (viewCommentParentDetailsDelegate != null
                    && commentParentEntity != null
                    && DetailsToolWindow.IsEntityTypeSupported(Utility.GetConcreteEntityType(commentParentEntity)))
                {
                    cm.Items.Add(new MenuItem
                    {
                        Header = "View parent details (DblClick)",
                        Command = new DelegatedCommand(viewCommentParentDetailsDelegate)
                    });
                }

                // open in browser
                if (openInBrowserDelegate != null)
                {
                    cm.Items.Add(new MenuItem
                    {
                        Header = "Open in Browser (Alt + DblClick)",
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
                            Header = "Copy Commit Message to Clipboard (Shift+Alt+DblClick)",
                            Command = new DelegatedCommand(copyCommitMessageDelegate)
                        });
                    }
                }

                // download gherkin script
                if (downloadGherkinScriptDelegate != null
                    && Utility.GetConcreteEntityType(selectedItem.Entity) == TestGherkin.SUBTYPE_GHERKIN_TEST)
                {
                    cm.Items.Add(new MenuItem
                    {
                        Header = "Download Script",
                        Command = new DelegatedCommand(downloadGherkinScriptDelegate)
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to show context menu.\n\n" + "Failed with message: " + ex.Message, AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static BaseEntity GetTaskParentEntity(BaseEntity entity)
        {
            if (entity.TypeName == Task.TYPE_TASK)
            {
                return (BaseEntity)entity.GetValue("story");
            }

            return null;
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
