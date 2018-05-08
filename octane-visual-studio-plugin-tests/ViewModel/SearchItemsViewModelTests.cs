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

        private static Story _story;
        private static Epic _epic;
        private static TestGherkin _gherkinTest;


        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            _guid = Guid.NewGuid();
            _story = StoryUtilities.CreateStory(EntityService, WorkspaceContext, "Story_" + _guid);
            _epic = EpicUtilities.CreateEpic(EntityService, WorkspaceContext, "Epic_" + _guid);
            _gherkinTest = TestGherkinUtilities.CreateGherkinTest(EntityService, WorkspaceContext, "TestGherkin_" + _guid);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            EntityService.DeleteById<Story>(WorkspaceContext, _story.Id);
            EntityService.DeleteById<Epic>(WorkspaceContext, _epic.Id);
            EntityService.DeleteById<TestGherkin>(WorkspaceContext, _gherkinTest.Id);
        }

        [TestMethod]
        public void SearchItemsViewModelTests_Search_EmptyFilter_NoSearchResults()
        {
            var viewModel = new SearchItemsViewModel("");
            viewModel.Search().Wait();

            Assert.AreEqual(0, viewModel.SearchItems.Count());
        }

        [TestMethod]
        public void SearchItemsViewModelTests_Search_Filter_Success()
        {
            var viewModel = new SearchItemsViewModel(_guid.ToString());

            Utility.WaitUntil(() =>
            {
                viewModel.Search().Wait();
                return viewModel.SearchItems.Count() == 3;
            }, "Timeout waiting for correct search results", new TimeSpan(0, 2, 0), new TimeSpan(0, 0, 1));

            Assert.AreEqual(1, viewModel.SearchItems.Count(si => si.ID == _story.Id), "Expected story wasn't returned by search operation.");
            Assert.AreEqual(1, viewModel.SearchItems.Count(si => si.ID == _epic.Id), "Expected epic wasn't returned by search operation.");
            Assert.AreEqual(1, viewModel.SearchItems.Count(si => si.ID == _gherkinTest.Id), "Expected gherkin test wasn't returned by search operation.");
        }
    }
}
