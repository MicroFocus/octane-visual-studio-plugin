﻿/*!
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

using MicroFocus.Adm.Octane.Api.Core.Entities;
using MicroFocus.Adm.Octane.VisualStudio.Tests.Utilities;
using MicroFocus.Adm.Octane.VisualStudio.Tests.Utilities.Entity;
using MicroFocus.Adm.Octane.VisualStudio.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
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
        private static Guid _quotesGuid;
        private static Guid _refreshGuid;

        private static Story _story;
        private static Epic _epic;
        private static TestGherkin _gherkinTest;

        private static Story _storyQuote;
        private static Story _storyDoubleQuote;

        private static Story _refreshStory;
        private static Epic _refreshEpic;
        private static TestGherkin _refreshGherkinTest;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            _guid = Guid.NewGuid();
            _story = StoryUtilities.CreateStory("Story_" + _guid);
            _epic = EpicUtilities.CreateEpic("Epic_" + _guid);
            _gherkinTest = TestGherkinUtilities.CreateGherkinTest("TestGherkin_" + _guid);

            _quotesGuid = Guid.NewGuid();
            _storyQuote = StoryUtilities.CreateStory("Story_\"_SingleQuote_" + _quotesGuid);
            _storyDoubleQuote = StoryUtilities.CreateStory("Story_\"\"_DoubleQuote_" + _quotesGuid);

            _refreshGuid = Guid.NewGuid();
            _refreshStory = StoryUtilities.CreateStory("Story_" + _refreshGuid);
            _refreshEpic = EpicUtilities.CreateEpic("Epic_" + _refreshGuid);
            _refreshGherkinTest = TestGherkinUtilities.CreateGherkinTest("TestGherkin_" + _refreshGuid);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            EntityService.DeleteById<Story>(WorkspaceContext, _story.Id);
            EntityService.DeleteById<Epic>(WorkspaceContext, _epic.Id);
            EntityService.DeleteById<TestGherkin>(WorkspaceContext, _gherkinTest.Id);

            EntityService.DeleteById<Story>(WorkspaceContext, _storyQuote.Id);
            EntityService.DeleteById<Story>(WorkspaceContext, _storyDoubleQuote.Id);

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
        public void SearchItemsViewModelTests_Search_EmptySpace_NoSearchResults()
        {
            var viewModel = new SearchItemsViewModel(" ");
            viewModel.SearchAsync().Wait();

            Assert.AreEqual(0, viewModel.SearchItems.Count(), "Searching for empty string should return nothing.");
        }

        [TestMethod]
        public void SearchItemsViewModelTests_Search_QuoteFilter_Success()
        {
            var viewModel = new SearchItemsViewModel("_\"_SingleQuote_" + _quotesGuid);
            ValidateSearch(viewModel, new List<BaseEntity> { _storyQuote });
        }

        [TestMethod]
        public void SearchItemsViewModelTests_Search_DoubleQuoteFilter_Success()
        {
            var viewModel = new SearchItemsViewModel("_\"\"_DoubleQuote_" + _quotesGuid);
            ValidateSearch(viewModel, new List<BaseEntity> { _storyDoubleQuote });
        }

        [TestMethod]
        public void SearchItemsViewModelTests_Search_Filter_Success()
        {
            var viewModel = new SearchItemsViewModel(_guid.ToString());
            ValidateSearch(viewModel, new List<BaseEntity> { _story, _epic, _gherkinTest });

            Assert.AreEqual(WindowMode.Loaded, viewModel.Mode, "Mismatched window mode");
            Assert.AreEqual(null, viewModel.ErrorMessage, "Mismatched error message");
        }

        #endregion

        #region Refresh

        [TestMethod]
        public void SearchItemsViewModelTests_Refresh_NoChanges_SameResults()
        {
            var viewModel = new SearchItemsViewModel(_guid.ToString());
            ValidateSearch(viewModel, new List<BaseEntity> { _story, _epic, _gherkinTest });

            viewModel.RefreshCommand.Execute(null);
            ValidateSearch(viewModel, new List<BaseEntity> { _story, _epic, _gherkinTest });
        }

        [TestMethod]
        public void SearchItemsViewModelTests_Refresh_Changes_Success()
        {
            var viewModel = new SearchItemsViewModel(_refreshGuid.ToString());
            ValidateSearch(viewModel, new List<BaseEntity> { _refreshStory, _refreshEpic, _refreshGherkinTest });

            var newEpic = EpicUtilities.CreateEpic("Epic2_" + _refreshGuid);
            try
            {
                viewModel.RefreshCommand.Execute(null);
                ValidateSearch(viewModel, new List<BaseEntity> { _refreshStory, _refreshEpic, _refreshGherkinTest, newEpic });
            }
            finally
            {
                EntityService.DeleteById<Epic>(WorkspaceContext, newEpic.Id);
            }
        }

        #endregion

        private static void ValidateSearch(SearchItemsViewModel viewModel, List<BaseEntity> expectedEntities)
        {
            Utility.WaitUntil(() =>
            {
                viewModel.SearchAsync().Wait();

                if (viewModel.SearchItems.Count() != expectedEntities.Count)
                    return false;

                foreach (var entity in expectedEntities)
                {
                    if (viewModel.SearchItems.Count(si => si.ID == entity.Id && si.Entity.Name == entity.Name) != 1)
                        return false;
                }

                return true;
            }, "Timeout waiting for correct search results", new TimeSpan(0, 2, 0), new TimeSpan(0, 0, 1));
        }
    }
}
