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
using System.Linq;

namespace MicroFocus.Adm.Octane.VisualStudio.Tests.ViewModel
{
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

        [TestMethod]
        public void DetailedItemViewModelTests_Test()
        {
            var viewModel = new DetailedItemViewModel(_story, MyWorkMetadata);
            viewModel.Initialize().Wait();

            Assert.AreEqual(DetailsWindowMode.ItemLoaded, viewModel.Mode, "Detailed item should have been loaded");
        }

        [TestMethod]
        public void DetailedItemViewModelTests_Filter()
        {
            var viewModel = new DetailedItemViewModel(_story, MyWorkMetadata);
            viewModel.Initialize().Wait();

            viewModel.Filter = "creat";

            Assert.AreEqual(1, viewModel.DisplayedEntityFields.Count());
        }


        [TestMethod]
        public void DetailedItemViewModelTests_CheckVisibleFields()
        {
            var viewModel = new DetailedItemViewModel(_story, MyWorkMetadata);
            viewModel.Initialize().Wait();

            var displayedEntityFields = viewModel.DisplayedEntityFields.ToList();

            var x = displayedEntityFields.Count(f => f.IsSelected);

            foreach (var field in displayedEntityFields)
            {
                field.IsSelected = true;
                viewModel.CheckboxChangeCommand.Execute(null);
            }

            var expectedVisibleFields = displayedEntityFields.Where(f => f.IsSelected).ToList();
            var actualVisibleFields = viewModel.VisibleFields.Where(f => f.IsSelected).ToList();
            CollectionAssert.AreEqual(expectedVisibleFields, actualVisibleFields);
        }
    }

}
