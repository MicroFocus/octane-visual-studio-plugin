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
using System.Threading;

namespace MicroFocus.Adm.Octane.VisualStudio.Tests.ViewModel
{
    /// <summary>
    /// Test class for <see cref="SearchItemsViewModel"/>
    /// </summary>
    [TestClass]
    public class SearchItemsViewModelTests : BaseOctanePluginTest
    {
        private static Story _story;
        private static Guid _guid;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            _guid = Guid.NewGuid();
            _story = StoryUtilities.CreateStory(EntityService, WorkspaceContext, "Story_" + _guid);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            EntityService.DeleteById<Story>(WorkspaceContext, _story.Id);
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

            SpinWait.SpinUntil(() =>
            {
                Thread.Sleep(1000);

                viewModel.Search().Wait();
                return viewModel.SearchItems.Count() == 1;
            }, new TimeSpan(0, 2, 0));

            Assert.AreEqual(1, viewModel.SearchItems.Count());
        }
    }
}
