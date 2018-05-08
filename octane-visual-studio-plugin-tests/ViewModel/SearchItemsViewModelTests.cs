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
using MicroFocus.Adm.Octane.VisualStudio.Tests.Utilities;
using MicroFocus.Adm.Octane.VisualStudio.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace MicroFocus.Adm.Octane.VisualStudio.Tests.ViewModel
{
    /// <summary>
    /// Test class for <see cref="SearchItemsViewModel"/>
    /// </summary>
    [TestClass]
    public class SearchItemsViewModelTests : BaseOctanePluginTest
    {
        private static Guid _guid;
        private static Guid _refreshGuid;

        private static Story _story;
        private static Epic _epic;
        private static TestGherkin _gherkinTest;

        private static Story _refreshStory;
        private static Epic _refreshEpic;
        private static TestGherkin _refreshGherkinTest;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            _guid = Guid.NewGuid();
            _story = StoryUtilities.CreateStory(EntityService, WorkspaceContext, "Story_" + _guid);
            _epic = EpicUtilities.CreateEpic(EntityService, WorkspaceContext, "Epic_" + _guid);
            _gherkinTest = TestGherkinUtilities.CreateGherkinTest(EntityService, WorkspaceContext, "TestGherkin_" + _guid);

            _refreshGuid = Guid.NewGuid();
            _refreshStory = StoryUtilities.CreateStory(EntityService, WorkspaceContext, "Story_" + _refreshGuid);
            _refreshEpic = EpicUtilities.CreateEpic(EntityService, WorkspaceContext, "Epic_" + _refreshGuid);
            _refreshGherkinTest = TestGherkinUtilities.CreateGherkinTest(EntityService, WorkspaceContext, "TestGherkin_" + _refreshGuid);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            EntityService.DeleteById<Story>(WorkspaceContext, _story.Id);
            EntityService.DeleteById<Epic>(WorkspaceContext, _epic.Id);
            EntityService.DeleteById<TestGherkin>(WorkspaceContext, _gherkinTest.Id);

            EntityService.DeleteById<Story>(WorkspaceContext, _refreshStory.Id);
            EntityService.DeleteById<Epic>(WorkspaceContext, _refreshEpic.Id);
            EntityService.DeleteById<TestGherkin>(WorkspaceContext, _refreshGherkinTest.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SearchItemsViewModelTests_Constructor_NullFilter_Exception()
        {
            new SearchItemsViewModel(null);
        }

        #region Search

        [TestMethod]
        public void SearchItemsViewModelTests_Search_EmptyFilter_NoSearchResults()
        {
            var viewModel = new SearchItemsViewModel("");
            viewModel.SearchAsync().Wait();

            Assert.AreEqual(0, viewModel.SearchItems.Count(), "Searching for empty string should return nothing.");
        }

        [TestMethod]
        public void SearchItemsViewModelTests_Search_QuoteFilter_Success()
        {
            SearchSpecialCharacters("\"");
        }

        [TestMethod]
        public void SearchItemsViewModelTests_Search_DoubleQuoteFilter_Success()
        {
            SearchSpecialCharacters("\"\"");
        }

        private static void SearchSpecialCharacters(string filter)
        {
            var story = StoryUtilities.CreateStory(EntityService, WorkspaceContext, "Story" + filter);
            try
            {
                var viewModel = new SearchItemsViewModel(filter);
                Utility.WaitUntil(() =>
                {
                    viewModel.SearchAsync().Wait();
                    return viewModel.SearchItems.Count(si => si.ID == story.Id) == 1;
                }, "Timeout waiting for correct search results", new TimeSpan(0, 2, 0), new TimeSpan(0, 0, 1));
            }
            finally
            {
                EntityService.DeleteById<Story>(WorkspaceContext, story.Id);
            }
        }

        [TestMethod]
        public void SearchItemsViewModelTests_Search_Filter_Success()
        {
            var viewModel = new SearchItemsViewModel(_guid.ToString());

            Utility.WaitUntil(() =>
            {
                viewModel.SearchAsync().Wait();
                return viewModel.SearchItems.Count() == 3;
            }, "Timeout waiting for correct search results", new TimeSpan(0, 2, 0), new TimeSpan(0, 0, 1));

            Assert.AreEqual(1, viewModel.SearchItems.Count(si => si.ID == _story.Id), "Expected story wasn't returned by search operation.");
            Assert.AreEqual(1, viewModel.SearchItems.Count(si => si.ID == _epic.Id), "Expected epic wasn't returned by search operation.");
            Assert.AreEqual(1, viewModel.SearchItems.Count(si => si.ID == _gherkinTest.Id), "Expected gherkin test wasn't returned by search operation.");

            Assert.AreEqual(WindowMode.Loaded, viewModel.Mode, "Mismatched window mode");
            Assert.AreEqual(null, viewModel.ErrorMessage, "Mismatched error message");
        }

        #endregion

        #region Refresh

        [TestMethod]
        public void SearchItemsViewModelTests_Refresh_NoChanges_SameResults()
        {
            var viewModel = new SearchItemsViewModel(_guid.ToString());

            Utility.WaitUntil(() =>
            {
                viewModel.SearchAsync().Wait();
                return viewModel.SearchItems.Count() == 3;
            }, "Timeout waiting for correct initial search results", new TimeSpan(0, 2, 0), new TimeSpan(0, 0, 1));

            Assert.AreEqual(1, viewModel.SearchItems.Count(si => si.ID == _story.Id), "Expected story wasn't returned by search operation.");
            Assert.AreEqual(1, viewModel.SearchItems.Count(si => si.ID == _epic.Id), "Expected epic wasn't returned by search operation.");
            Assert.AreEqual(1, viewModel.SearchItems.Count(si => si.ID == _gherkinTest.Id), "Expected gherkin test wasn't returned by search operation.");

            var expectedSearchItems = viewModel.SearchItems.Select(si => si.ID).ToList();

            viewModel.RefreshCommand.Execute(null);

            Utility.WaitUntil(() =>
            {
                viewModel.SearchAsync().Wait();
                return viewModel.SearchItems.Count() == 3;
            }, "Timeout waiting for correct search results after refresh", new TimeSpan(0, 2, 0), new TimeSpan(0, 0, 1));

            CollectionAssert.AreEqual(expectedSearchItems, viewModel.SearchItems.Select(si => si.ID).ToList(),
                "Mismatched search results after refresh");
        }

        [TestMethod]
        public void SearchItemsViewModelTests_Refresh_Changes_Success()
        {
            var viewModel = new SearchItemsViewModel(_refreshGuid.ToString());

            Utility.WaitUntil(() =>
            {
                viewModel.SearchAsync().Wait();
                return viewModel.SearchItems.Count() == 3;
            }, "Timeout waiting for correct initial search results", new TimeSpan(0, 2, 0), new TimeSpan(0, 0, 1));

            Assert.AreEqual(1, viewModel.SearchItems.Count(si => si.ID == _refreshStory.Id), "Expected story wasn't returned by search operation.");
            Assert.AreEqual(1, viewModel.SearchItems.Count(si => si.ID == _refreshEpic.Id), "Expected epic wasn't returned by search operation.");
            Assert.AreEqual(1, viewModel.SearchItems.Count(si => si.ID == _refreshGherkinTest.Id), "Expected gherkin test wasn't returned by search operation.");

            var newEpic = EpicUtilities.CreateEpic(EntityService, WorkspaceContext, "Epic2_" + _refreshGuid);

            viewModel.RefreshCommand.Execute(null);

            Utility.WaitUntil(() =>
            {
                viewModel.SearchAsync().Wait();
                return viewModel.SearchItems.Count() == 4;
            }, "Timeout waiting for correct search results after refresh", new TimeSpan(0, 2, 0), new TimeSpan(0, 0, 1));

            Assert.AreEqual(1, viewModel.SearchItems.Count(si => si.ID == _refreshStory.Id), "Expected story wasn't returned by search operation.");
            Assert.AreEqual(1, viewModel.SearchItems.Count(si => si.ID == _refreshEpic.Id), "Expected epic wasn't returned by search operation.");
            Assert.AreEqual(1, viewModel.SearchItems.Count(si => si.ID == newEpic.Id), "Expected new epic wasn't returned by search operation.");
            Assert.AreEqual(1, viewModel.SearchItems.Count(si => si.ID == _refreshGherkinTest.Id), "Expected gherkin test wasn't returned by search operation.");
        }

        #endregion
    }
}
