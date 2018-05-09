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
            var story = StoryUtilities.CreateStory("Story_OctaneMyItemsViewModelTests");
            var defect = DefectUtilities.CreateDefect("Defect_OctaneMyItemsViewModelTests");
            try
            {
                var viewModel = new OctaneMyItemsViewModel();
                viewModel.LoadMyItemsAsync().Wait();

                Assert.AreEqual(1, viewModel.MyItems.Count(), "Mismatched MyItems count");
            }
            finally
            {
                EntityService.DeleteById<Story>(WorkspaceContext, story.Id);
            }
        }
    }
}
