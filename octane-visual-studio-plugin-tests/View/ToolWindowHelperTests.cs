﻿/*!
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
using MicroFocus.Adm.Octane.VisualStudio.Tests.Utilities.Entity;
using MicroFocus.Adm.Octane.VisualStudio.View;
using MicroFocus.Adm.Octane.VisualStudio.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace MicroFocus.Adm.Octane.VisualStudio.Tests.View
{
    /// <summary>
    /// Test class for <see cref="ToolWindowHelper"/>
    /// </summary>
    [TestClass]
    public class ToolWindowHelperTests : BaseOctanePluginTest
    {
        private static int _value;

        private const int ViewDetailsValue = 1;
        private readonly Action<object> _viewDetailsDelegate = x => _value = ViewDetailsValue;
        private const int ViewTaskParentDetailsValue = 2;
        private readonly Action<object> _viewTaskParentDetailsDelegate = x => _value = ViewTaskParentDetailsValue;
        private const int ViewCommentParentDetailsValue = 3;
        private readonly Action<object> _viewCommentParentDetailsDelegate = x => _value = ViewCommentParentDetailsValue;
        private const int OpenInBrowserValue = 4;
        private readonly Action<object> _openInBrowserDelegate = x => _value = OpenInBrowserValue;
        private const int CopyCommitMessageValue = 5;
        private readonly Action<object> _copyCommitMessageDelegate = x => _value = CopyCommitMessageValue;
        private const int DownloadGherkinScriptValue = 6;
        private readonly Action<object> _downloadGherkinScriptDelegate = x => _value = DownloadGherkinScriptValue;

        /// <inheritdoc/>>
        protected override void TestInitializeInternal()
        {
            _value = 0;
        }

        [TestMethod]
        public void ToolWindowHelperTests_ConstructContextMenu_NullContextMenu_Success()
        {
            ToolWindowHelper.ConstructContextMenu(null, null, null, null, null, null, null, null);
        }

        [TestMethod]
        public void ToolWindowHelperTests_ConstructContextMenu_NullSelectedItem_Success()
        {
            var cm = new ContextMenu();
            cm.Items.Add(new MenuItem());

            ToolWindowHelper.ConstructContextMenu(cm, null, null, null, null, null, null, null);

            Assert.AreEqual(0, cm.Items.Count, "Mismatched number of menu items in context menu");
        }

        [TestMethod]
        public void ToolWindowHelperTests_ConstructContextMenu_Story_Success()
        {
            ValidateContextMenuItems(StoryUtilities.CreateStory(),
                new List<MenuItemEnum>
                {
                    MenuItemEnum.ViewDetails,
                    MenuItemEnum.OpenInBrowser,
                    MenuItemEnum.CopyCommitMessage
                });
        }

        [TestMethod]
        public void ToolWindowHelperTests_ConstructContextMenu_Task_Success()
        {
            var story = StoryUtilities.CreateStory();
            try
            {
                ValidateContextMenuItems(TaskUtilities.CreateTask(story),
                    new List<MenuItemEnum>
                    {
                        MenuItemEnum.ViewDetails,
                        MenuItemEnum.TaskViewParentDetails,
                        MenuItemEnum.OpenInBrowser,
                        MenuItemEnum.CopyCommitMessage
                    });
            }
            finally
            {
                EntityService.DeleteById<Story>(WorkspaceContext, story.Id);
            }
        }

        [TestMethod]
        public void ToolWindowHelperTests_ConstructContextMenu_QualityStory_Success()
        {
            ValidateContextMenuItems(QualityStoryUtilities.CreateQualityStory(),
                new List<MenuItemEnum>
                {
                    MenuItemEnum.ViewDetails,
                    MenuItemEnum.OpenInBrowser,
                    MenuItemEnum.CopyCommitMessage
                });
        }

        [TestMethod]
        public void ToolWindowHelperTests_ConstructContextMenu_Defect_Success()
        {
            ValidateContextMenuItems(DefectUtilities.CreateDefect(),
                new List<MenuItemEnum>
                {
                    MenuItemEnum.ViewDetails,
                    MenuItemEnum.OpenInBrowser,
                    MenuItemEnum.CopyCommitMessage
                });
        }

        [TestMethod]
        public void ToolWindowHelperTests_ConstructContextMenu_GherkinTest_Success()
        {
            ValidateContextMenuItems(TestGherkinUtilities.CreateGherkinTest(),
                new List<MenuItemEnum>
                {
                    MenuItemEnum.ViewDetails,
                    MenuItemEnum.OpenInBrowser,
                    MenuItemEnum.DownloadScript
                });
        }

        [TestMethod]
        public void ToolWindowHelperTests_ConstructContextMenu_ManualTest_Success()
        {
            ValidateContextMenuItems(TestManualUtilities.CreateManualTest(),
                new List<MenuItemEnum>
                {
                    MenuItemEnum.ViewDetails,
                    MenuItemEnum.OpenInBrowser
                });
        }

        [TestMethod]
        public void ToolWindowHelperTests_ConstructContextMenu_ManualRun_Success()
        {
            var manualTest = TestManualUtilities.CreateManualTest();
            try
            {
                ValidateContextMenuItems(RunManualUtilities.CreateManualRun(manualTest),
                    new List<MenuItemEnum>
                    {
                        MenuItemEnum.ViewDetails,
                        MenuItemEnum.OpenInBrowser,
                    });
            }
            finally
            {
                EntityService.DeleteById<TestManual>(WorkspaceContext, manualTest.Id);
            }
        }

        [TestMethod]
        public void ToolWindowHelperTests_ConstructContextMenu_SuiteRun_Success()
        {
            var testSuite = TestSuiteUtilities.CreateTestSuite();
            try
            {
                ValidateContextMenuItems(RunSuiteUtilities.CreateSuiteRun(testSuite),
                    new List<MenuItemEnum>
                    {
                        MenuItemEnum.ViewDetails,
                        MenuItemEnum.OpenInBrowser,
                    });
            }
            finally
            {
                EntityService.DeleteById<TestSuite>(WorkspaceContext, testSuite.Id);
            }
        }

        [TestMethod]
        public void ToolWindowHelperTests_ConstructContextMenu_Epic_Success()
        {
            ValidateContextMenuItems(EpicUtilities.CreateEpic(),
                new List<MenuItemEnum>
                {
                    MenuItemEnum.OpenInBrowser,
                }, false);
        }

        [TestMethod]
        public void ToolWindowHelperTests_ConstructContextMenu_Feature_Success()
        {
            var epic = EpicUtilities.CreateEpic();
            try
            {
                ValidateContextMenuItems(FeatureUtilities.CreateFeature(epic),
                    new List<MenuItemEnum>
                    {
                        MenuItemEnum.OpenInBrowser,
                    }, false);
            }
            finally
            {
                EntityService.DeleteById<Epic>(WorkspaceContext, epic.Id);
            }
        }

        // TODO remove useMyItems and use a more generic mechanism to obtain the BaseItemViewModel
        private void ValidateContextMenuItems<T>(T entity, List<MenuItemEnum> expectedMenuItems, bool useMyItems = true) where T : BaseEntity
        {
            try
            {
                BaseItemViewModel selectedItem;
                if (useMyItems)
                {
                    var viewModel = new OctaneMyItemsViewModel();
                    viewModel.LoadMyItemsAsync().Wait();

                    selectedItem = viewModel.MyItems.FirstOrDefault(i => i.ID == entity.Id);
                }
                else
                {
                    selectedItem = new BaseItemViewModel(entity);
                }

                Assert.IsNotNull(selectedItem, "Couldn't find entity");

                var cm = new ContextMenu();

                ToolWindowHelper.ConstructContextMenu(cm, selectedItem,
                    _viewDetailsDelegate,
                    _viewTaskParentDetailsDelegate,
                    _viewCommentParentDetailsDelegate,
                    _openInBrowserDelegate,
                    _copyCommitMessageDelegate,
                    _downloadGherkinScriptDelegate);

                Assert.AreEqual(expectedMenuItems.Count, cm.Items.Count,
                    "Mismatched number of menu items in context menu");

                var items = new MenuItem[cm.Items.Count];
                cm.Items.CopyTo(items, 0);

                int index = 0;
                foreach (var item in expectedMenuItems)
                {
                    switch (item)
                    {
                        case MenuItemEnum.ViewDetails:
                            ValidateMenuItem(items, index, ToolWindowHelper.ViewDetailsHeader, ViewDetailsValue);
                            break;
                        case MenuItemEnum.TaskViewParentDetails:
                            ValidateMenuItem(items, index, ToolWindowHelper.ViewTaskParentDetailsHeader,
                                ViewTaskParentDetailsValue);
                            break;
                        case MenuItemEnum.CommentViewParentDetails:
                            ValidateMenuItem(items, index, ToolWindowHelper.CopyCommitMessageHeader,
                                ViewCommentParentDetailsValue);
                            break;
                        case MenuItemEnum.OpenInBrowser:
                            ValidateMenuItem(items, index, ToolWindowHelper.OpenInBrowserHeader, OpenInBrowserValue);
                            break;
                        case MenuItemEnum.CopyCommitMessage:
                            ValidateMenuItem(items, index, ToolWindowHelper.CopyCommitMessageHeader, CopyCommitMessageValue);
                            break;
                        case MenuItemEnum.DownloadScript:
                            ValidateMenuItem(items, index, ToolWindowHelper.DownloadGherkinScriptHeader,
                                DownloadGherkinScriptValue);
                            break;
                    }

                    index++;
                }
            }
            finally
            {
                EntityService.DeleteById<T>(WorkspaceContext, entity.Id);
            }
        }

        private void ValidateMenuItem(MenuItem[] items, int expectedIndex, string expectedHeader, int expectedCommandValue)
        {
            var menuItem = items[expectedIndex];
            Assert.AreEqual(expectedHeader, menuItem.Header.ToString(),
                    $"Couldn't find expected menu item at index {expectedIndex}");

            menuItem.Command.Execute(null);

            Assert.AreEqual(expectedCommandValue, _value, $"Mismatched command value for header \"{expectedHeader}\"");

            // reset test value
            _value = 0;
        }

        private enum MenuItemEnum
        {
            ViewDetails,
            TaskViewParentDetails,
            CommentViewParentDetails,
            OpenInBrowser,
            CopyCommitMessage,
            DownloadScript
        };
    }
}