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
using MicroFocus.Adm.Octane.VisualStudio.Tests.Utilities.Entity;
using MicroFocus.Adm.Octane.VisualStudio.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace MicroFocus.Adm.Octane.VisualStudio.Tests.ViewModel
{
    /// <summary>
    /// Test class for <see cref="OctaneMyItemsViewModel"/>
    /// </summary>
    [TestClass]
    public class OctaneMyItemsViewModelTests : BaseOctanePluginTest
    {
        [TestMethod]
        public void OctaneMyItemsViewModelTests_Constructor_DefaultValues_Success()
        {
            var viewModel = new OctaneMyItemsViewModel();
            Assert.AreEqual(MainWindowMode.FirstTime, viewModel.Mode, "Mismatched mode");
            Assert.IsTrue(string.IsNullOrEmpty(viewModel.LastExceptionMessage), "Mismatched exception message");
            Assert.AreEqual(0, viewModel.MyItems.Count(), "Mismatched MyItems count");
        }

        [TestMethod]
        public void OctaneMyItemsViewModelTests_MyItems_AllSupportedEntityTypes_Success()
        {
            var story = StoryUtilities.CreateStory();
            var qualityStory = QualityStoryUtilities.CreateQualityStory();
            var defect = DefectUtilities.CreateDefect();

            var task = TaskUtilities.CreateTask(story);

            var gherkinTest = TestGherkinUtilities.CreateGherkinTest();
            var manualTest = TestManualUtilities.CreateManualTest();

            var expectedItems = new List<BaseEntity> { story, qualityStory, task, defect, gherkinTest, manualTest };
            try
            {
                var viewModel = new OctaneMyItemsViewModel();
                viewModel.LoadMyItemsAsync().Wait();

                var myItems = viewModel.MyItems.ToList();
                Assert.IsTrue(myItems.Count >= expectedItems.Count, "Mismatched MyItems count");

                foreach (var item in expectedItems)
                {
                    Assert.AreEqual(1, myItems.Count(i => i.ID == item.Id), $"Couldn't find entity {item.Name}");
                }
            }
            finally
            {
                EntityService.DeleteById<Task>(WorkspaceContext, task.Id);

                EntityService.DeleteById<Story>(WorkspaceContext, story.Id);
                EntityService.DeleteById<QualityStory>(WorkspaceContext, qualityStory.Id);
                EntityService.DeleteById<Defect>(WorkspaceContext, defect.Id);

                EntityService.DeleteById<TestGherkin>(WorkspaceContext, gherkinTest.Id);
                EntityService.DeleteById<TestManual>(WorkspaceContext, manualTest.Id);
            }
        }

        [TestMethod]
        public void OctaneMyItemsViewModelTests_MyItems_NotSupportedEntityTypes_Success()
        {
            var epic = EpicUtilities.CreateEpic();
            var feature = FeatureUtilities.CreateFeature(epic);

            var expectedItems = new List<BaseEntity> { epic, feature };
            try
            {
                var viewModel = new OctaneMyItemsViewModel();
                viewModel.LoadMyItemsAsync().Wait();

                var myItems = viewModel.MyItems.ToList();
                foreach (var item in expectedItems)
                {
                    Assert.AreEqual(0, myItems.Count(i => i.ID == item.Id), $"Found unsupported entity {item.Name}");
                }
            }
            finally
            {
                EntityService.DeleteById<Epic>(WorkspaceContext, epic.Id);
                EntityService.DeleteById<Feature>(WorkspaceContext, feature.Id);
            }
        }
    }
}
