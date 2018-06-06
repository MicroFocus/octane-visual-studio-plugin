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
using System;
using System.Collections.Generic;
using System.Linq;

namespace MicroFocus.Adm.Octane.VisualStudio.Tests.ViewModel
{
    /// <summary>
    /// Test class for <see cref="OctaneItemViewModel"/>
    /// </summary>
    [TestClass]
    public class OctaneItemViewModelTests : BaseOctanePluginTest
    {
        private static Story _storyEntity;

        private static OctaneItemViewModel _storyViewModel;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            _storyEntity = StoryUtilities.CreateStory();

            var viewModel = new OctaneMyItemsViewModel();
            viewModel.LoadMyItemsAsync().Wait();

            _storyViewModel = viewModel.MyItems.FirstOrDefault(i => i.ID == _storyEntity.Id && i.Entity.Name == _storyEntity.Name);
            Assert.IsNotNull(_storyViewModel, "Couldn't find story entity in MyWork");
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            EntityService.DeleteById<Story>(WorkspaceContext, _storyEntity.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void OctaneItemViewModelTests_Constructor_NullEntity_Throws()
        {
            new OctaneItemViewModel(null);
        }

        [TestMethod]
        public void OctaneItemViewModelTests_Fields_ValidateAllFields_Success()
        {
            Assert.AreEqual(CommonFields.Release, _storyViewModel.SubTitleField.Name);

            ValidateFields(_storyViewModel.TopFields.ToList(), new List<string>
            {
                CommonFields.Phase, CommonFields.StoryPoints, CommonFields.Owner, CommonFields.Author
            });
            ValidateFields(_storyViewModel.BottomFields.ToList(), new List<string>
            {
                CommonFields.InvestedHours, CommonFields.RemainingHours, CommonFields.EstimatedHours
            });
        }

        private void ValidateFields(List<object> actualFields, List<string> expectedValues)
        {
            for (int i = 0; i < expectedValues.Count; i++)
            {
                var fieldViewModel = actualFields[i * 2] as FieldViewModel;
                Assert.IsNotNull(fieldViewModel, $"Element for {expectedValues} should be a FieldViewModel");
                Assert.AreEqual(expectedValues[i], fieldViewModel.Name, "Mismatched field name");

                if (i != expectedValues.Count - 1)
                {
                    var separatorViewModel = actualFields[i * 2 + 1] as SeparatorViewModel;
                    Assert.IsNotNull(separatorViewModel, $"Element after {expectedValues} should be a SeparatorViewModel");
                }
            }
        }
    }
}
