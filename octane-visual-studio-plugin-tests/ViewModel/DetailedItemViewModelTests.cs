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
using MicroFocus.Adm.Octane.VisualStudio.Tests.Utilities;
using MicroFocus.Adm.Octane.VisualStudio.Tests.Utilities.Entity;
using MicroFocus.Adm.Octane.VisualStudio.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Utility = MicroFocus.Adm.Octane.VisualStudio.Common.Utility;

namespace MicroFocus.Adm.Octane.VisualStudio.Tests.ViewModel
{
    /// <summary>
    /// Test class for <see cref="DetailedItemViewModel"/>
    /// </summary>
    [TestClass]
    public class DetailedItemViewModelTests : BaseOctanePluginTest
    {
        private static Story _story;
        private static Task _task;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            _story = StoryUtilities.CreateStory();
            _task = TaskUtilities.CreateTask(_story);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            EntityService.DeleteById<Story>(WorkspaceContext, _story.Id);
            EntityService.DeleteById<Task>(WorkspaceContext, _task.Id);
        }

        #region EntitySupportsComments

        [TestMethod]
        public void DetailedItemViewModelTests_EntitySupportsComments_EntitySupportsComments_True()
        {
            var viewModel = new DetailedItemViewModel(_story);
            Assert.IsTrue(viewModel.EntitySupportsComments, "Entity should support comments");
        }

        [TestMethod]
        public void DetailedItemViewModelTests_EntitySupportsComments_EntityDoesntSupportComments_False()
        {
            var task = new Task("1001");
            task.SetValue(WorkItem.SUBTYPE_FIELD, "task");
            var viewModel = new DetailedItemViewModel(task);
            Assert.IsFalse(viewModel.EntitySupportsComments, "Entity shouldn't support comments");
        }

        #endregion

        #region SaveEntityCommand

        [TestMethod]
        public void DetailedItemViewModelTests_SaveEntityCommand_ChangeIntField_Success()
        {
            var viewModel = new DetailedItemViewModel(_story);
            viewModel.InitializeAsync().Wait();

            var storyPointsField = viewModel.VisibleFields.FirstOrDefault(f => f.Name == CommonFields.StoryPoints);
            storyPointsField.Content = 1234;
            Assert.AreEqual(1234, storyPointsField.Content);

            viewModel.SaveEntityCommand.Execute(null);

            Utilities.Utility.WaitUntil(() => viewModel.Mode == WindowMode.Loaded,
                "Timeout while refreshing the entity", new TimeSpan(0, 0, 30));

            storyPointsField = viewModel.VisibleFields.FirstOrDefault(f => f.Name == CommonFields.StoryPoints);
            Assert.AreEqual(1234, storyPointsField.Content);
        }

        [TestMethod]
        public void DetailedItemViewModelTests_SaveEntityCommand_ChangePhaseForStory_Success()
        {
            ValidateChangePhase(_story);
        }

        [TestMethod]
        public void DetailedItemViewModelTests_SaveEntityCommand_ChangePhaseForTask_Success()
        {
            ValidateChangePhase(_task);
        }

        private void ValidateChangePhase(BaseEntity entity)
        {
            var viewModel = new DetailedItemViewModel(entity);
            viewModel.InitializeAsync().Wait();

            Assert.IsNull(viewModel.SelectedNextPhase, "SelectedNextPhase should be null after initialization");

            for (int i = 0; i < 5; i++)
            {
                Assert.IsTrue(viewModel.NextPhaseNames.Count > 0, "There should be at least one next phase name");
                var nextPhaseName = viewModel.NextPhaseNames[0];
                viewModel.SelectedNextPhase = nextPhaseName;

                viewModel.SaveEntityCommand.Execute(null);

                Utilities.Utility.WaitUntil(() => viewModel.Mode == WindowMode.Loaded,
                    "Timeout while refreshing the entity for next phase " + nextPhaseName, new TimeSpan(0, 0, 30));

                Assert.AreEqual(nextPhaseName, viewModel.Phase, "Mismatched entity phase after save");
                Assert.IsNull(viewModel.SelectedNextPhase, "SelectedNextPhase should be null after save");
            }
        }

        [TestMethod]
        public void DetailedItemViewModelTests_SaveEntityCommand_Name_Success()
        {
            var viewModel = new DetailedItemViewModel(_story);
            viewModel.InitializeAsync().Wait();

            var newName = "New_Story_" + Guid.NewGuid();
            viewModel.Title = newName;
            Assert.AreEqual(newName, viewModel.Title, "Mismatched entity name after setting it");

            viewModel.SaveEntityCommand.Execute(null);

            Utilities.Utility.WaitUntil(() => viewModel.Mode == WindowMode.Loaded,
                "Timeout while refreshing the entity", new TimeSpan(0, 0, 30));

            Assert.AreEqual(newName, viewModel.Title, "Mismatched entity name after save");
        }

        #endregion

        #region RefreshCommand

        [TestMethod]
        public void DetailedItemViewModelTests_RefreshCommand_RefreshWithoutAnyChanges_Success()
        {
            var viewModel = new DetailedItemViewModel(_story);
            viewModel.InitializeAsync().Wait();

            var expectedVisibleFields = viewModel.VisibleFields.Select(f => f.Name).ToList();

            viewModel.RefreshCommand.Execute(null);

            Utilities.Utility.WaitUntil(() => viewModel.Mode == WindowMode.Loaded,
                "Timeout while refreshing the entity", new TimeSpan(0, 0, 30));

            var actualVisibleFields = viewModel.VisibleFields.Select(f => f.Name).ToList();

            CollectionAssert.AreEqual(expectedVisibleFields, actualVisibleFields, "Mismathed visible fields");
        }

        [TestMethod]
        public void DetailedItemViewModelTests_RefreshCommand_RefreshAfterChangingVisibleFields_Succes()
        {
            var viewModel = new DetailedItemViewModel(_story);
            viewModel.InitializeAsync().Wait();

            foreach (var field in viewModel.FilteredEntityFields)
            {
                field.IsSelected = true;
                viewModel.ToggleEntityFieldVisibilityCommand.Execute(null);
            }

            var expectedVisibleFields = viewModel.VisibleFields.Select(f => f.Name).ToList();

            viewModel.RefreshCommand.Execute(null);

            Utilities.Utility.WaitUntil(() => viewModel.Mode == WindowMode.Loaded,
                "Timeout while refreshing the entity", new TimeSpan(0, 0, 30));

            var actualVisibleFields = viewModel.VisibleFields.Select(f => f.Name).ToList();

            CollectionAssert.AreEqual(expectedVisibleFields, actualVisibleFields, "Mismathed visible fields");
        }

        #endregion

        #region Filter

        [TestMethod]
        public void DetailedItemViewModelTests_Filter_NullFilter_ReturnAllFields()
        {
            var viewModel = new DetailedItemViewModel(_story);
            viewModel.InitializeAsync().Wait();

            var expectedFilteredFields = viewModel.FilteredEntityFields.Select(f => f.Name).ToList();

            viewModel.Filter = null;

            var actualFilteredFields = viewModel.FilteredEntityFields.Select(f => f.Name).ToList();

            CollectionAssert.AreEqual(expectedFilteredFields, actualFilteredFields, "Mismathed filtered fields");
        }

        [TestMethod]
        public void DetailedItemViewModelTests_Filter_EmptyFilter_ReturnAllFields()
        {
            var viewModel = new DetailedItemViewModel(_story);
            viewModel.InitializeAsync().Wait();

            var expectedFilteredFields = viewModel.FilteredEntityFields.Select(f => f.Name).ToList();

            viewModel.Filter = string.Empty;

            var actualFilteredFields = viewModel.FilteredEntityFields.Select(f => f.Name).ToList();

            CollectionAssert.AreEqual(expectedFilteredFields, actualFilteredFields, "Mismathed filtered fields");
        }

        [TestMethod]
        public void DetailedItemViewModelTests_Filter_FilterDoesntMatchAnyItem_ReturnEmptyList()
        {
            var viewModel = new DetailedItemViewModel(_story);
            viewModel.InitializeAsync().Wait();

            viewModel.Filter = "FilterDoesntMatchAnyItem";

            var actualFilteredFields = viewModel.FilteredEntityFields.Select(f => f.Name).ToList();

            Assert.AreEqual(0, actualFilteredFields.Count,
                "Filter that doesn't match any item should return an empty search result");
        }

        [TestMethod]
        public void DetailedItemViewModelTests_Filter_FilterPartialMatch_ReturnMatchList()
        {
            var viewModel = new DetailedItemViewModel(_story);
            viewModel.InitializeAsync().Wait();

            var expectedFilteredFields = new List<string> { "Blocked reason", "Creation time", "Feature", "Release", "Team" };
            viewModel.Filter = "ea";
            var actualFilteredFields = viewModel.FilteredEntityFields.Select(f => f.Label).ToList();
            CollectionAssert.AreEqual(expectedFilteredFields, actualFilteredFields,
                $"Mismathed filtered fields for filter '{viewModel.Filter}'");

            expectedFilteredFields = new List<string> { "Creation time", "Feature", };
            viewModel.Filter = "eat";
            actualFilteredFields = viewModel.FilteredEntityFields.Select(f => f.Label).ToList();
            CollectionAssert.AreEqual(expectedFilteredFields, actualFilteredFields,
                $"Mismathed filtered fields for filter '{viewModel.Filter}'");

            expectedFilteredFields = new List<string> { "Feature" };
            viewModel.Filter = "feature";
            actualFilteredFields = viewModel.FilteredEntityFields.Select(f => f.Label).ToList();
            CollectionAssert.AreEqual(expectedFilteredFields, actualFilteredFields,
                $"Mismathed filtered fields for filter '{viewModel.Filter}'");
        }

        [TestMethod]
        public void DetailedItemViewModelTests_Filter_FilterIgnoreCase_ReturnMatchList()
        {
            var viewModel = new DetailedItemViewModel(_story);
            viewModel.InitializeAsync().Wait();

            var expectedFilteredFields = new List<string> { "Creation time", "Feature", };
            viewModel.Filter = "EaT";
            var actualFilteredFields = viewModel.FilteredEntityFields.Select(f => f.Label).ToList();
            CollectionAssert.AreEqual(expectedFilteredFields, actualFilteredFields,
                $"Mismathed filtered fields for filter 'EaT'");
        }

        #endregion

        #region VisibleFields

        [TestMethod]
        public void DetailedItemViewModelTests_VisibleFields_ShowHideFields_ShowSelectedFields()
        {
            var viewModel = new DetailedItemViewModel(_story);
            viewModel.InitializeAsync().Wait();

            ChangeFieldVisibility(viewModel, "Release", false);
            ChangeFieldVisibility(viewModel, "Committers", true);

            var expectedVisibleFields = viewModel.FilteredEntityFields.Where(f => f.IsSelected).Select(f => f.Name).OrderBy(f => f).ToList();
            var actualVisibleFields = viewModel.VisibleFields.Select(f => f.Name).OrderBy(f => f).ToList();
            CollectionAssert.AreEqual(expectedVisibleFields, actualVisibleFields, "Mismathed visible fields");
        }

        [TestMethod]
        public void DetailedItemViewModelTests_VisibleFields_ShowAllFields_ShowSelectedFields()
        {
            var viewModel = new DetailedItemViewModel(_story);
            viewModel.InitializeAsync().Wait();

            foreach (var field in viewModel.FilteredEntityFields)
            {
                field.IsSelected = true;
                viewModel.ToggleEntityFieldVisibilityCommand.Execute(null);
            }

            var expectedVisibleFields = viewModel.FilteredEntityFields.Where(f => f.IsSelected).Select(f => f.Name).OrderBy(f => f).ToList();
            var actualVisibleFields = viewModel.VisibleFields.Select(f => f.Name).OrderBy(f => f).ToList();
            CollectionAssert.AreEqual(expectedVisibleFields, actualVisibleFields, "Mismathed visible fields");
        }

        [TestMethod]
        public void DetailedItemViewModelTests_VisibleFields_HideAllFields_ShowSelectedFields()
        {
            var viewModel = new DetailedItemViewModel(_story);
            viewModel.InitializeAsync().Wait();

            foreach (var field in viewModel.FilteredEntityFields)
            {
                field.IsSelected = false;
                viewModel.ToggleEntityFieldVisibilityCommand.Execute(null);
            }

            var expectedVisibleFields = viewModel.FilteredEntityFields.Where(f => f.IsSelected).Select(f => f.Name).ToList();
            var actualVisibleFields = viewModel.VisibleFields.Select(f => f.Name).ToList();
            CollectionAssert.AreEqual(expectedVisibleFields, actualVisibleFields, "Mismathed visible fields");
        }

        [TestMethod]
        public void DetailedItemViewModelTests_VisibleFields_MultipleEntitiesOfSameTime_ChangesAreReflectedInAllEntities()
        {
            var viewModel = new DetailedItemViewModel(_story);
            viewModel.InitializeAsync().Wait();

            var secondStory = StoryUtilities.CreateStory();
            try
            {
                var secondViewModel = new DetailedItemViewModel(_story);
                secondViewModel.InitializeAsync().Wait();

                ChangeFieldVisibility(viewModel, "Release", false);
                ChangeFieldVisibility(viewModel, "Committers", true);

                var expectedVisibleFields = viewModel.VisibleFields.Select(f => f.Name).OrderBy(f => f).ToList();
                var actualVisibleFields = secondViewModel.FilteredEntityFields.Where(f => f.IsSelected).Select(f => f.Name).OrderBy(f => f).ToList();
                CollectionAssert.AreEqual(expectedVisibleFields, actualVisibleFields, "Mismathed visible fields");
            }
            finally
            {
                EntityService.DeleteById<Story>(WorkspaceContext, secondStory.Id);
            }
        }

        #endregion

        #region DefaultFields

        [TestMethod]
        public void DetailedItemViewModelTests_ResetFieldsCustomizationCommand_AllFieldsAreVisible_ReturnToDefault()
        {
            ValidateResetCommand(true);
        }

        [TestMethod]
        public void DetailedItemViewModelTests_ResetFieldsCustomizationCommand_NoFieldIsVisible_ReturnToDefault()
        {
            ValidateResetCommand(false);
        }

        private void ValidateResetCommand(bool allFieldsVisible)
        {
            var viewModel = new DetailedItemViewModel(_story);
            viewModel.InitializeAsync().Wait();

            Assert.IsTrue(viewModel.OnlyDefaultFieldsAreShown, "Only default fields should be visible when detailed item is initialized");

            foreach (var field in viewModel.FilteredEntityFields)
            {
                field.IsSelected = allFieldsVisible;
                viewModel.ToggleEntityFieldVisibilityCommand.Execute(null);
            }

            Assert.IsFalse(viewModel.OnlyDefaultFieldsAreShown, "Visible fields should be different than default fields");

            viewModel.ResetFieldsCustomizationCommand.Execute(null);

            var dynamicFieldsCache = ExposedClass.From(typeof(FieldsCache));
            var cache = dynamicFieldsCache._defaultFieldsCache as FieldsCache.Metadata;

            var persistedVisibleFields = cache.data[Utility.GetConcreteEntityType(_story)];

            Assert.AreEqual(persistedVisibleFields.Count, viewModel.VisibleFields.Count(), "Inconsistent number of visible fields");
            foreach (var field in viewModel.VisibleFields)
            {
                Assert.IsTrue(persistedVisibleFields.Contains(field.Name), $"Field {field.Name} should be visible");
            }

            Assert.IsTrue(viewModel.OnlyDefaultFieldsAreShown, "Only default fields should be visible after reset command");
        }

        [TestMethod]
        public void DetailedItemViewModelTests_OnlyDefaultFieldsAreShown_ToggleShowingOnlyDefaultFields_True()
        {
            var viewModel = new DetailedItemViewModel(_story);
            viewModel.InitializeAsync().Wait();

            Assert.IsTrue(viewModel.OnlyDefaultFieldsAreShown, "Only default fields should be visible when detailed item is initialized");

            ChangeFieldVisibility(viewModel, "Release", false);
            Assert.IsFalse(viewModel.OnlyDefaultFieldsAreShown, "Not all default fields are visible");

            ChangeFieldVisibility(viewModel, "Release", true);
            Assert.IsTrue(viewModel.OnlyDefaultFieldsAreShown, "All default fields should be visible");

            ChangeFieldVisibility(viewModel, "Committers", true);
            Assert.IsFalse(viewModel.OnlyDefaultFieldsAreShown, "More fields than the default fields are visible");

            ChangeFieldVisibility(viewModel, "Committers", false);
            Assert.IsTrue(viewModel.OnlyDefaultFieldsAreShown, "All default fields should be visible");
        }

        #endregion

        private void ChangeFieldVisibility(DetailedItemViewModel viewModel, string fieldLabel, bool visibility)
        {
            var field = viewModel.FilteredEntityFields.FirstOrDefault(f => f.Label == fieldLabel);
            field.IsSelected = visibility;
            viewModel.ToggleEntityFieldVisibilityCommand.Execute(null);
        }

        #region CommentSectionVisibility

        [TestMethod]
        public void DetailedItemViewModelTests_CommentSectionVisibility_ToggleCommentSectionCommand_Success()
        {
            var viewModel = new DetailedItemViewModel(_story);
            Assert.IsFalse(viewModel.CommentSectionVisibility, "Default value for CommentSectionVisibility should be false");
            Assert.AreEqual(DetailedItemViewModel.ShowCommentsTooltip, viewModel.ShowCommentTooltip, "Mismatched default ShowCommentTooltip");

            viewModel.ToggleCommentSectionCommand.Execute(null);
            Assert.IsTrue(viewModel.CommentSectionVisibility, "Executing ToggleCommentSectionCommand should change CommentSectionVisibility to true");
            Assert.AreEqual(DetailedItemViewModel.HideCommentsTooltip, viewModel.ShowCommentTooltip, "Mismatched ShowCommentTooltip after executing ToggleCommentSectionCommand");

            viewModel.ToggleCommentSectionCommand.Execute(null);
            Assert.IsFalse(viewModel.CommentSectionVisibility, "Executing ToggleCommentSectionCommand again should change CommentSectionVisibility to false");
            Assert.AreEqual(DetailedItemViewModel.ShowCommentsTooltip, viewModel.ShowCommentTooltip, "Mismatched ShowCommentTooltip after executing ToggleCommentSectionCommand again");
        }

        #endregion


        #region AddComment
        [TestMethod]
        public void DetailedItemViewModelTests_AddComment_Succes()
        {
            var viewModel = new DetailedItemViewModel(_story);
            viewModel.InitializeAsync().Wait();
            string textForComment = Guid.NewGuid().ToString();
            viewModel.CommentText = "<html><body>" + textForComment + "</body></html>";
            Assert.AreNotEqual("", viewModel.CommentText);

            viewModel.AddCommentCommand.Execute(null);
            Thread.Sleep(2048);
            int detectedCommentsWithText = 0;
            foreach(var comment in viewModel.Comments)
            {
                if (comment.Text == textForComment)
                {
                    detectedCommentsWithText++;
                }
            }
            Assert.AreEqual(1, detectedCommentsWithText);

            var commentFromStory = viewModel.Comments.First();
            Assert.AreEqual(commentFromStory.Text, textForComment);
        }
        #endregion 

        [TestMethod]
        public void DetailedItemViewModelTests_VariousProperties_BeforeAndAfterInitialize_Success()
        {
            var viewModel = new DetailedItemViewModel(_story);
            Assert.AreEqual(WindowMode.Loading, viewModel.Mode, "Mismatched initial mode");

            viewModel.InitializeAsync().Wait();
            Assert.AreEqual(WindowMode.Loaded, viewModel.Mode, "Mismatched mode after initialization");

            var entityTypeInformation = EntityTypeRegistry.GetEntityTypeInformation(_story);
            Assert.AreEqual(entityTypeInformation.ShortLabel, viewModel.IconText, "Mismatched icon text");
            Assert.AreEqual(entityTypeInformation.Color, viewModel.IconBackgroundColor, "Mismatched icon background color");
        }

       
        //TODO: Fix this - Error is 403 forbidden: EXTENSION_TO_MIME_TYPE is the problem I think, because it says 
        //something regarding the text/plain content type and we try to upload something of type .txt (not sure) 
        public void DetailedItemViewModelTests_HandleImagesInDescription_DownloadImage_Success()
        {
            var fileName = "DetailedItemViewModelTests_HandleImagesInDescription_" + Guid.NewGuid() + ".txt";

            var fileContentsBytes = new byte[2500];
            var rnd = new Random();
            rnd.NextBytes(fileContentsBytes);

            Api.Core.Connector.RestConnector.AwaitContinueOnCapturedContext = false;
            // simulating uploading a picture; using plain text to also test content randomness
            var attachment = EntityService.AttachToEntity(WorkspaceContext, _story, fileName, fileContentsBytes, "text/plain", new string[] { "owner_work_item" });
            Assert.IsNotNull(attachment.Id, "Attachment id shouldn't be null");
            Assert.AreEqual(WorkItem.SUBTYPE_STORY, attachment.owner_work_item.TypeName, "Mismatched attachment parent type");
            Assert.AreEqual(_story.Id, attachment.owner_work_item.Id, "Mismatched attachment parent id");

            var updatedStory = new Story(_story.Id);
            updatedStory.SetValue(CommonFields.Description,
                "<html><body>" +
                "<div style=\"\">" +
                "<img data-size-percentage=\"100\" src=\"" + WorkspaceContext.GetPath() + "/attachments/" + attachment.Id + "/" + fileName + "\" style=\"width:437px;height:303px;\" />" +
                "</div>" +
                "<p>&nbsp;</p>" +
                "</body></html>");
            updatedStory = EntityService.Update(WorkspaceContext, updatedStory, new[] { "name", "subtype", CommonFields.Description });

            var viewModel = new DetailedItemViewModel(updatedStory);
            viewModel.InitializeAsync().Wait();

            var path = DetailedItemViewModel.TempPath + WorkItem.SUBTYPE_STORY + updatedStory.Id + fileName;
            ValidateFileContents(path, fileContentsBytes);

            var fileInfo = new FileInfo(path);
            var expectedLastWriteTime = fileInfo.LastWriteTime;

            Thread.Sleep(1000);

            viewModel.RefreshCommand.Execute(null);

            fileInfo = new FileInfo(path);
            Assert.AreEqual(expectedLastWriteTime, fileInfo.LastWriteTime, "Downloaded attachement was modified by refresh");
        }

        private void ValidateFileContents(string filePath, byte[] expectedContentBytes)
        {
            byte[] buffer = new byte[1024];
            using (var stream = new FileStream(filePath, FileMode.Open))
            using (var memoryStream = new MemoryStream())
            {
                // copy the stream contents to a memory stream
                int size = stream.Read(buffer, 0, buffer.Length);
                while (size > 0)
                {
                    memoryStream.Write(buffer, 0, size);
                    size = stream.Read(buffer, 0, buffer.Length);
                }

                CollectionAssert.AreEqual(expectedContentBytes, memoryStream.ToArray(), "Mismatched attachment contents");
            }
        }
    }
}
