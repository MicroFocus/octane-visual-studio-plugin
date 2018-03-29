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
using MicroFocus.Adm.Octane.Api.Core.Tests;
using MicroFocus.Adm.Octane.VisualStudio.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace MicroFocus.Adm.Octane.VisualStudio.Tests.ViewModel
{
    /// <summary>
    /// Test class for <see cref="DetailedItemViewModel"/>
    /// </summary>
    [TestClass]
    public class DetailedItemViewModelTests : BaseOctanePluginTest
    {
        private static Story _story;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            _story = StoryUtilities.CreateStory(entityService, workspaceContext);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            entityService.DeleteById<Story>(workspaceContext, _story.Id);
        }

        #region EntitySupportsComments

        [TestMethod]
        public void DetailedItemViewModelTests_EntitySupportsComments_EntitySupportsComments_True()
        {
            var viewModel = new DetailedItemViewModel(_story, MyWorkMetadata);
            Assert.IsTrue(viewModel.EntitySupportsComments, "Entity should support comments");
        }

        [TestMethod]
        public void DetailedItemViewModelTests_EntitySupportsComments_EntityDoesntSupportComments_False()
        {
            var viewModel = new DetailedItemViewModel(new Task(), MyWorkMetadata);
            Assert.IsFalse(viewModel.EntitySupportsComments, "Entity shouldn't support comments");
        }

        #endregion

        #region RefreshCommand

        [TestMethod]
        public void DetailedItemViewModelTests_RefreshCommand_RefreshWithoutAnyChanges_Success()
        {
            var viewModel = new DetailedItemViewModel(_story, MyWorkMetadata);
            viewModel.Initialize().Wait();

            var expectedVisibleFields = viewModel.VisibleFields.Select(f => f.Name).ToList();

            viewModel.RefreshCommand.Execute(null);

            TestUtilities.WaitUntil(() => viewModel.Mode == DetailsWindowMode.ItemLoaded, 5000,
                "Timeout while refreshing the entity");

            var actualVisibleFields = viewModel.VisibleFields.Select(f => f.Name).ToList();

            CollectionAssert.AreEqual(expectedVisibleFields, actualVisibleFields, "Mismathed visible fields");
        }

        [TestMethod]
        public void DetailedItemViewModelTests_RefreshCommand_RefreshAfterChangingVisibleFields_Succes()
        {
            var viewModel = new DetailedItemViewModel(_story, MyWorkMetadata);
            viewModel.Initialize().Wait();

            foreach (var field in viewModel.FilteredEntityFields)
            {
                field.IsSelected = true;
                viewModel.CheckboxChangeCommand.Execute(null);
            }

            var expectedVisibleFields = viewModel.VisibleFields.Select(f => f.Name).ToList();

            viewModel.RefreshCommand.Execute(null);

            TestUtilities.WaitUntil(() => viewModel.Mode == DetailsWindowMode.ItemLoaded, 5000,
                "Timeout while refreshing the entity");

            var actualVisibleFields = viewModel.VisibleFields.Select(f => f.Name).ToList();

            CollectionAssert.AreEqual(expectedVisibleFields, actualVisibleFields, "Mismathed visible fields");
        }

        #endregion

        #region Filter

        [TestMethod]
        public void DetailedItemViewModelTests_Filter_NullFilter_ReturnAllFields()
        {
            var viewModel = new DetailedItemViewModel(_story, MyWorkMetadata);
            viewModel.Initialize().Wait();

            var expectedFilteredFields = viewModel.FilteredEntityFields.Select(f => f.Name).ToList();

            viewModel.Filter = null;

            var actualFilteredFields = viewModel.FilteredEntityFields.Select(f => f.Name).ToList();

            CollectionAssert.AreEqual(expectedFilteredFields, actualFilteredFields, "Mismathed filtered fields");
        }

        [TestMethod]
        public void DetailedItemViewModelTests_Filter_EmptyFilter_ReturnAllFields()
        {
            var viewModel = new DetailedItemViewModel(_story, MyWorkMetadata);
            viewModel.Initialize().Wait();

            var expectedFilteredFields = viewModel.FilteredEntityFields.Select(f => f.Name).ToList();

            viewModel.Filter = string.Empty;

            var actualFilteredFields = viewModel.FilteredEntityFields.Select(f => f.Name).ToList();

            CollectionAssert.AreEqual(expectedFilteredFields, actualFilteredFields, "Mismathed filtered fields");
        }

        [TestMethod]
        public void DetailedItemViewModelTests_Filter_FilterDoesntMatchAnyItem_ReturnEmptyList()
        {
            var viewModel = new DetailedItemViewModel(_story, MyWorkMetadata);
            viewModel.Initialize().Wait();

            viewModel.Filter = "FilterDoesntMatchAnyItem";

            var actualFilteredFields = viewModel.FilteredEntityFields.Select(f => f.Name).ToList();

            Assert.AreEqual(0, actualFilteredFields.Count,
                "Filter that doesn't match any item should return an empty search result");
        }

        [TestMethod]
        public void DetailedItemViewModelTests_Filter_FilterPartialMatch_ReturnMatchList()
        {
            var viewModel = new DetailedItemViewModel(_story, MyWorkMetadata);
            viewModel.Initialize().Wait();

            var expectedFilteredFields = new List<string> { "Creation time", "Feature", "Release", "Team", "Blocked reason" };
            viewModel.Filter = "ea";
            var actualFilteredFields = viewModel.FilteredEntityFields.Select(f => f.Label).ToList();
            CollectionAssert.AreEqual(expectedFilteredFields, actualFilteredFields,
                $"Mismathed filtered fields for filter '{viewModel.Filter}'");

            expectedFilteredFields = new List<string> { "Creation time", "Feature", };
            viewModel.Filter = "eat";
            actualFilteredFields = viewModel.FilteredEntityFields.Select(f => f.Label).ToList();
            CollectionAssert.AreEqual(expectedFilteredFields, actualFilteredFields,
                $"Mismathed filtered fields for filter '{viewModel.Filter}'");

            expectedFilteredFields = new List<string> { "Feature" };
            viewModel.Filter = "feature";
            actualFilteredFields = viewModel.FilteredEntityFields.Select(f => f.Label).ToList();
            CollectionAssert.AreEqual(expectedFilteredFields, actualFilteredFields,
                $"Mismathed filtered fields for filter '{viewModel.Filter}'");
        }

        [TestMethod]
        public void DetailedItemViewModelTests_Filter_FilterIgnoreCase_ReturnMatchList()
        {
            var viewModel = new DetailedItemViewModel(_story, MyWorkMetadata);
            viewModel.Initialize().Wait();

            var expectedFilteredFields = new List<string> { "Creation time", "Feature", };
            viewModel.Filter = "EaT";
            var actualFilteredFields = viewModel.FilteredEntityFields.Select(f => f.Label).ToList();
            CollectionAssert.AreEqual(expectedFilteredFields, actualFilteredFields,
                $"Mismathed filtered fields for filter 'EaT'");
        }

        #endregion

        #region VisibleFields

        [TestMethod]
        public void DetailedItemViewModelTests_VisibleFields_ShowHideFields_ShowSelectedFields()
        {
            var viewModel = new DetailedItemViewModel(_story, MyWorkMetadata);
            viewModel.Initialize().Wait();

            var releaseField = viewModel.FilteredEntityFields.FirstOrDefault(f => f.Label == "Release");
            releaseField.IsSelected = false;
            viewModel.CheckboxChangeCommand.Execute(null);

            var commitersField = viewModel.FilteredEntityFields.FirstOrDefault(f => f.Label == "Committers");
            commitersField.IsSelected = true;
            viewModel.CheckboxChangeCommand.Execute(null);

            var expectedVisibleFields = viewModel.FilteredEntityFields.Where(f => f.IsSelected).Select(f => f.Name).ToList();
            var actualVisibleFields = viewModel.VisibleFields.Where(f => f.IsSelected).Select(f => f.Name).ToList();
            CollectionAssert.AreEqual(expectedVisibleFields, actualVisibleFields, "Mismathed visible fields");
        }

        [TestMethod]
        public void DetailedItemViewModelTests_VisibleFields_ShowAllFields_ShowSelectedFields()
        {
            var viewModel = new DetailedItemViewModel(_story, MyWorkMetadata);
            viewModel.Initialize().Wait();

            foreach (var field in viewModel.FilteredEntityFields)
            {
                field.IsSelected = true;
                viewModel.CheckboxChangeCommand.Execute(null);
            }

            var expectedVisibleFields = viewModel.FilteredEntityFields.Where(f => f.IsSelected).Select(f => f.Name).ToList();
            var actualVisibleFields = viewModel.VisibleFields.Where(f => f.IsSelected).Select(f => f.Name).ToList();
            CollectionAssert.AreEqual(expectedVisibleFields, actualVisibleFields, "Mismathed visible fields");
        }

        [TestMethod]
        public void DetailedItemViewModelTests_VisibleFields_HideAllFields_ShowSelectedFields()
        {
            var viewModel = new DetailedItemViewModel(_story, MyWorkMetadata);
            viewModel.Initialize().Wait();

            foreach (var field in viewModel.FilteredEntityFields)
            {
                field.IsSelected = false;
                viewModel.CheckboxChangeCommand.Execute(null);
            }

            var expectedVisibleFields = viewModel.FilteredEntityFields.Where(f => f.IsSelected).Select(f => f.Name).ToList();
            var actualVisibleFields = viewModel.VisibleFields.Where(f => f.IsSelected).Select(f => f.Name).ToList();
            CollectionAssert.AreEqual(expectedVisibleFields, actualVisibleFields, "Mismathed visible fields");
        }

        #endregion
    }
}
