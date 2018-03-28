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
using MicroFocus.Adm.Octane.VisualStudio.Common;
using MicroFocus.Adm.Octane.VisualStudio.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace MicroFocus.Adm.Octane.VisualStudio.Tests.ViewModel
{
    [TestClass]
    public class DetailedItemViewModelTests : BaseTest
    {
        private readonly MyWorkMetadata _workMetadata = new MyWorkMetadata();

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            InitConnection(context);

            OctaneConfiguration.Url = host;
            OctaneConfiguration.Username = userName;
            OctaneConfiguration.Password = password;
            OctaneConfiguration.WorkSpaceId = workspaceContext.WorkspaceId;
            OctaneConfiguration.SharedSpaceId = sharedSpaceContext.SharedSpaceId;
        }

        [TestMethod]
        public void DetailedItemViewModelTests_Test()
        {
            var entity = new Story()
            {
                Id = "1002",
                SubType = WorkItem.SUBTYPE_STORY
            };

            var viewModel = new DetailedItemViewModel(entity, _workMetadata);

            viewModel.Initialize().Wait();

            Assert.AreEqual(DetailsWindowMode.ItemLoaded, viewModel.Mode, "Detailed item should have been loaded");
        }

        [TestMethod]
        public void DetailedItemViewModelTests_Filter()
        {
            var entity = new Story()
            {
                Id = "1002",
                SubType = WorkItem.SUBTYPE_STORY
            };

            var viewModel = new DetailedItemViewModel(entity, _workMetadata);

            viewModel.Initialize().Wait();

            viewModel.Filter = "creat";

            Assert.AreEqual(1, viewModel.DisplayedEntityFields.Count());
        }
    }

}
