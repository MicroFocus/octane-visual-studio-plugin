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
using MicroFocus.Adm.Octane.VisualStudio.Common;
using MicroFocus.Adm.Octane.VisualStudio.Tests.Utilities.Entity;
using MicroFocus.Adm.Octane.VisualStudio.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MicroFocus.Adm.Octane.VisualStudio.Tests.Common
{
    /// <summary>
    /// Test class for <see cref="FieldsMetadataService"/>
    /// </summary>
    [TestClass]
    public class FieldsMetadataServiceTests : BaseOctanePluginTest
    {
        private static Story _story;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            _story = StoryUtilities.CreateStory();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            EntityService.DeleteById<Story>(WorkspaceContext, _story.Id);
        }

        #region GetFieldMetadata

        [TestMethod]
        public void FieldsMetadataServiceTests_GetFieldMetadata_CallAfterReset_ReturnsMetadata1()
        {
            var metadata = FieldsMetadataService.GetFieldMetadata(_story).Result;
            Assert.IsTrue(metadata.Count > 0, "Cache should contain field metadata for user story entity");

            var defect = new Defect
            {
                SubType = Defect.SUBTYPE_DEFECT
            };
            metadata = FieldsMetadataService.GetFieldMetadata(defect).Result;
            Assert.IsTrue(metadata.Count > 0, "Cache should contain field metadata for user defect");
        }

        #endregion
    }
}
