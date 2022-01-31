/*!
* © Copyright 2017-2022 Micro Focus or one of its affiliates.
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
using MicroFocus.Adm.Octane.VisualStudio.Tests.Utilities.Entity;
using MicroFocus.Adm.Octane.VisualStudio.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MicroFocus.Adm.Octane.VisualStudio.Tests.ViewModel
{
    /// <summary>
    /// Test class for <see cref="OctaneItemViewModel"/>
    /// </summary>
    [TestClass]
    public class OctaneItemViewModelTests : BaseOctanePluginTest
    {
        private static Story _storyEntity;
        private static Task _taskEntity;
        private static TestGherkin _gherkinTestEntity;

        private static OctaneItemViewModel _storyViewModel;
        private static OctaneItemViewModel _taskViewModel;
        private static OctaneItemViewModel _gherkinTestViewModel;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            _storyEntity = StoryUtilities.CreateStory();
            _taskEntity = TaskUtilities.CreateTask(_storyEntity);
            _gherkinTestEntity = TestGherkinUtilities.CreateGherkinTest();

            var viewModel = new OctaneMyItemsViewModel();
            viewModel.LoadMyItemsAsync().Wait();

            _storyViewModel = viewModel.MyItems.FirstOrDefault(i => i.ID == _storyEntity.Id && i.Entity.Name == _storyEntity.Name);
            Assert.IsNotNull(_storyViewModel, "Couldn't find story entity in MyWork");

            _taskViewModel = viewModel.MyItems.FirstOrDefault(i => i.ID == _taskEntity.Id && i.Entity.Name == _taskEntity.Name);
            Assert.IsNotNull(_taskViewModel, "Couldn't find task entity in MyWork");

            _gherkinTestViewModel = viewModel.MyItems.FirstOrDefault(i => i.ID == _gherkinTestEntity.Id && i.Entity.Name == _gherkinTestEntity.Name);
            Assert.IsNotNull(_gherkinTestViewModel, "Couldn't find gherkin entity in MyWork");

            Assert.IsNull(OctaneItemViewModel.CurrentActiveItem, "There shouldn't be an active item");
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            EntityService.DeleteById<Task>(WorkspaceContext, _taskEntity.Id);
            EntityService.DeleteById<Story>(WorkspaceContext, _storyEntity.Id);
            EntityService.DeleteById<TestGherkin>(WorkspaceContext, _gherkinTestEntity.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void OctaneItemViewModelTests_Constructor_NullEntity_Throws()
        {
            new OctaneItemViewModel(null);
        }

        protected override void TestInitializeInternal()
        {
            OctaneItemViewModel.ClearActiveItem();
        }

        #region Fields

        [TestMethod]
        public void OctaneItemViewModelTests_Fields_ValidateAllFields_Success()
        {
            Assert.AreEqual(CommonFields.Release, _storyViewModel.SubTitleField.Name);

            ValidateFields(_storyViewModel.TopFields.ToList(), new List<string>
            {
                CommonFields.Phase, CommonFields.StoryPoints, CommonFields.Owner, CommonFields.Author
            });
            ValidateFields(_storyViewModel.BottomFields.ToList(), new List<string>
            {
                CommonFields.InvestedHours, CommonFields.RemainingHours, CommonFields.EstimatedHours
            });
        }

        private void ValidateFields(List<object> actualFields, List<string> expectedValues)
        {
            int actualFieldsCounter = 0;
            int expectedFieldsCounter = 0;
            for (int i = 0; i < expectedValues.Count; i++)
            {
                var fieldViewModel = actualFields[actualFieldsCounter++] as FieldViewModel;
                Assert.IsNotNull(fieldViewModel, $"Element for {expectedValues} should be a FieldViewModel");
                Assert.AreEqual(expectedValues[expectedFieldsCounter++], fieldViewModel.Name, "Mismatched field name");

                if(fieldViewModel.Name.Equals("story_points") && fieldViewModel.Content.Equals(""))
                {
                    var horizontalSeparatorViewModel = actualFields[actualFieldsCounter++] as HorizontalSeparatorViewModel;
                    Assert.IsNotNull(horizontalSeparatorViewModel, $"Element after {expectedValues} should be a HorizontalSeparatorViewModel");
                }

                if (i != expectedValues.Count - 1)
                {
                    var separatorViewModel = actualFields[actualFieldsCounter++] as SeparatorViewModel;
                    Assert.IsNotNull(separatorViewModel, $"Element after {expectedValues} should be a SeparatorViewModel");
                }
            }
        }

        #endregion

        #region ActiveItem

        [TestMethod]
        public void OctaneItemViewModelTests_SetActiveItem_NullItem_Success()
        {
            OctaneItemViewModel.SetActiveItem(null);
        }

        [TestMethod]
        public void OctaneItemViewModelTests_SetActiveItem_SetValidItem_Success()
        {
            Assert.IsNull(OctaneItemViewModel.CurrentActiveItem, "There shouldn't be an active item");
            Assert.IsFalse(_taskViewModel.IsActiveWorkItem, "Task item shouldn't be active");
            Assert.IsFalse(_storyViewModel.IsActiveWorkItem, "Story item shouldn't be active");

            OctaneItemViewModel.SetActiveItem(_storyViewModel);

            Assert.AreEqual(_storyViewModel, OctaneItemViewModel.CurrentActiveItem, "Story item should be the active item");
            Assert.IsTrue(_storyViewModel.IsActiveWorkItem, "Story item should be the active after SetActiveItem");
        }

        [TestMethod]
        public void OctaneItemViewModelTests_SetActiveItem_ChangeValidItem_Success()
        {
            Assert.IsNull(OctaneItemViewModel.CurrentActiveItem, "There shouldn't be an active item");
            Assert.IsFalse(_taskViewModel.IsActiveWorkItem, "Task item shouldn't be active");
            Assert.IsFalse(_storyViewModel.IsActiveWorkItem, "Story item shouldn't be active");

            OctaneItemViewModel.SetActiveItem(_taskViewModel);

            Assert.AreEqual(_taskViewModel, OctaneItemViewModel.CurrentActiveItem, "Task item should be the active item");
            Assert.IsTrue(_taskViewModel.IsActiveWorkItem, "Task item should be the active after calling SetActiveItem on task");

            OctaneItemViewModel.SetActiveItem(_storyViewModel);

            Assert.AreEqual(_storyViewModel, OctaneItemViewModel.CurrentActiveItem, "Story item should be the active item");
            Assert.IsFalse(_taskViewModel.IsActiveWorkItem, "Task item shouldn't be active after calling SetActiveItem on Story item");
            Assert.IsTrue(_storyViewModel.IsActiveWorkItem, "Story item should be active after calling SetActiveItem on Story item");
        }

        [TestMethod]
        public void OctaneItemViewModelTests_ClearActiveItem_ClearWhenThereIsNoActiveItem_Success()
        {
            Assert.IsNull(OctaneItemViewModel.CurrentActiveItem, "There shouldn't be an active item");
            Assert.IsFalse(_taskViewModel.IsActiveWorkItem, "Task item shouldn't be active");
            Assert.IsFalse(_storyViewModel.IsActiveWorkItem, "Story item shouldn't be active");

            OctaneItemViewModel.ClearActiveItem();

            Assert.IsNull(OctaneItemViewModel.CurrentActiveItem, "There shouldn't be an active item after ClearActiveItem");
            Assert.IsFalse(_taskViewModel.IsActiveWorkItem, "Task item shouldn't be active after ClearActiveItem");
            Assert.IsFalse(_storyViewModel.IsActiveWorkItem, "Story item shouldn't be active after ClearActiveItem");
        }

        [TestMethod]
        public void OctaneItemViewModelTests_ClearActiveItem_ClearActiveItem_Success()
        {
            Assert.IsNull(OctaneItemViewModel.CurrentActiveItem, "There shouldn't be an active item");
            Assert.IsFalse(_taskViewModel.IsActiveWorkItem, "Task item shouldn't be active");
            Assert.IsFalse(_storyViewModel.IsActiveWorkItem, "Story item shouldn't be active");

            OctaneItemViewModel.SetActiveItem(_storyViewModel);

            Assert.AreEqual(_storyViewModel, OctaneItemViewModel.CurrentActiveItem, "Story item should be the active item");

            OctaneItemViewModel.ClearActiveItem();

            Assert.IsNull(OctaneItemViewModel.CurrentActiveItem, "There shouldn't be an active item after ClearActiveItem");
            Assert.IsFalse(_taskViewModel.IsActiveWorkItem, "Task item shouldn't be active after ClearActiveItem");
            Assert.IsFalse(_storyViewModel.IsActiveWorkItem, "Story item shouldn't be active after ClearActiveItem");
        }

        #endregion

        #region CommitMessage

        [TestMethod]
        public void OctaneItemViewModelTests_CommitMessage_ForStory_Success()
        {
            Assert.AreEqual($"user story #{_storyEntity.Id}: ", _storyViewModel.CommitMessage, "Mismatched commit message");
        }

        [TestMethod]
        public void OctaneItemViewModelTests_CommitMessage_ForTask_Success()
        {
            Assert.AreEqual($"user story #{_storyEntity.Id}: task #{_taskEntity.Id}: ", _taskViewModel.CommitMessage, "Mismatched commit message");
        }

        [TestMethod]
        public void OctaneItemViewModelTests_CommitMessage_ForGherkinTest_Success()
        {
            Assert.AreEqual(string.Empty, _gherkinTestViewModel.CommitMessage, "Gherkin test doesn't support commit message");
        }

        #endregion
    }
}
