/*!
* © Copyright 2017-2022 Micro Focus or one of its affiliates.
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MicroFocus.Adm.Octane.VisualStudio.Tests.Common
{
    /// <summary>
    /// Test class for <see cref="WorkspaceSessionPersistanceManager"/>
    /// </summary>
    [TestClass]
    public class WorkspaceSessionPersistanceManagerTests : BaseOctanePluginTest
    {
        private string _url;
        private long _ssid;
        private long _wsid;
        private string _username;
        private string _password;

        protected override void TestInitializeInternal()
        {
            _url = OctaneConfiguration.Url;
            _ssid = OctaneConfiguration.SharedSpaceId;
            _wsid = OctaneConfiguration.WorkSpaceId;
            _username = OctaneConfiguration.Username;
            _password = OctaneConfiguration.Password;

            WorkspaceSessionPersistanceManager.ClearActiveEntity();
        }

        protected override void TestCleanupInternal()
        {
            OctaneConfiguration.Url = _url;
            OctaneConfiguration.SharedSpaceId = _ssid;
            OctaneConfiguration.WorkSpaceId = _wsid;
            OctaneConfiguration.Username = _username;
            OctaneConfiguration.Password = _password;

            WorkspaceSessionPersistanceManager.ClearActiveEntity();
        }

        #region History

        [TestMethod]
        public void WorkspaceSessionPersistanceManagerTests_UpdateHistory_NullFilter_Success()
        {
            OctaneConfiguration.Url = Guid.NewGuid().ToString();

            CollectionAssert.AreEqual(new List<string>(), WorkspaceSessionPersistanceManager.History, "Invalid initial history");

            WorkspaceSessionPersistanceManager.UpdateHistory("a");
            WorkspaceSessionPersistanceManager.UpdateHistory("b");
            var expectedHistory = new List<string> { "b", "a" };
            CollectionAssert.AreEqual(expectedHistory, WorkspaceSessionPersistanceManager.History, "Mismatched history");

            WorkspaceSessionPersistanceManager.UpdateHistory(string.Empty);

            CollectionAssert.AreEqual(expectedHistory, WorkspaceSessionPersistanceManager.History, "Mismatched history after trying to add null");
        }

        [TestMethod]
        public void WorkspaceSessionPersistanceManagerTests_UpdateHistory_EmptyFilter_Success()
        {
            OctaneConfiguration.Url = Guid.NewGuid().ToString();

            CollectionAssert.AreEqual(new List<string>(), WorkspaceSessionPersistanceManager.History, "Invalid initial history");

            WorkspaceSessionPersistanceManager.UpdateHistory("a");
            WorkspaceSessionPersistanceManager.UpdateHistory("b");
            var expectedHistory = new List<string> { "b", "a" };
            CollectionAssert.AreEqual(expectedHistory, WorkspaceSessionPersistanceManager.History, "Mismatched history");

            WorkspaceSessionPersistanceManager.UpdateHistory(string.Empty);

            CollectionAssert.AreEqual(expectedHistory, WorkspaceSessionPersistanceManager.History, "Mismatched history after trying to add empty string");
        }

        [TestMethod]
        public void WorkspaceSessionPersistanceManagerTests_UpdateHistory_SaveMoreElementsThanMax_Success()
        {
            OctaneConfiguration.Url = Guid.NewGuid().ToString();

            CollectionAssert.AreEqual(new List<string>(), WorkspaceSessionPersistanceManager.History, "Invalid initial history");

            var expectedHistory = new List<string>();
            for (var i = 0; i < WorkspaceSessionPersistanceManager.MaxSearchHistorySize + 2; i++)
            {
                expectedHistory.Add(i.ToString());
                WorkspaceSessionPersistanceManager.UpdateHistory(i.ToString());
            }

            expectedHistory.Reverse();
            expectedHistory = expectedHistory.Take(WorkspaceSessionPersistanceManager.MaxSearchHistorySize).ToList();

            CollectionAssert.AreEqual(expectedHistory, WorkspaceSessionPersistanceManager.History, "Mismatched history");
        }

        [TestMethod]
        public void WorkspaceSessionPersistanceManagerTests_UpdateHistory_SaveExactlyMaxElements_Success()
        {
            OctaneConfiguration.Url = Guid.NewGuid().ToString();

            CollectionAssert.AreEqual(new List<string>(), WorkspaceSessionPersistanceManager.History, "Invalid initial history");

            var expectedHistory = new List<string>();
            for (var i = 0; i < WorkspaceSessionPersistanceManager.MaxSearchHistorySize; i++)
            {
                expectedHistory.Add(i.ToString());
                WorkspaceSessionPersistanceManager.UpdateHistory(i.ToString());
            }

            expectedHistory.Reverse();

            CollectionAssert.AreEqual(expectedHistory, WorkspaceSessionPersistanceManager.History, "Mismatched history");
        }

        [TestMethod]
        public void WorkspaceSessionPersistanceManagerTests_UpdateHistory_ResetApplication_Success()
        {
            OctaneConfiguration.Url = Guid.NewGuid().ToString();

            CollectionAssert.AreEqual(new List<string>(), WorkspaceSessionPersistanceManager.History, "Invalid initial history");

            WorkspaceSessionPersistanceManager.UpdateHistory("1");

            var exposedHistoryManager = ExposedClass.From(typeof(WorkspaceSessionPersistanceManager));
            exposedHistoryManager._metadata = null;

            CollectionAssert.AreEqual(new List<string> { "1" }, WorkspaceSessionPersistanceManager.History, "Mismatched history after reset");

            WorkspaceSessionPersistanceManager.UpdateHistory("2");

            CollectionAssert.AreEqual(new List<string> { "2", "1" }, WorkspaceSessionPersistanceManager.History, "Mismatched history");
        }

        #endregion

        #region ActiveItem

        [TestMethod]
        public void WorkspaceSessionPersistanceManagerTests_IsActiveEntity_Null_Success()
        {
            Assert.IsFalse(WorkspaceSessionPersistanceManager.IsActiveEntity(null));
        }

        [TestMethod]
        public void WorkspaceSessionPersistanceManagerTests_SetActiveEntity_ValidEntity_Success()
        {
            ValidateActiveEntity(null);

            var story = new Story(123) { SubType = WorkItem.SUBTYPE_STORY };

            WorkspaceSessionPersistanceManager.SetActiveEntity(story);
            ValidateActiveEntity(story);

            WorkspaceSessionPersistanceManager.ClearActiveEntity();
            ValidateActiveEntity(null);
        }

        [TestMethod]
        public void WorkspaceSessionPersistanceManagerTests_SetActiveEntity_ChangeActiveEntity_Success()
        {
            ValidateActiveEntity(null);

            var story = new Story(123) { SubType = WorkItem.SUBTYPE_STORY };
            WorkspaceSessionPersistanceManager.SetActiveEntity(story);
            ValidateActiveEntity(story);

            var defect = new Defect(456) { SubType = WorkItem.SUBTYPE_DEFECT };
            WorkspaceSessionPersistanceManager.SetActiveEntity(defect);
            ValidateActiveEntity(defect);
        }

        #endregion

        #region ContextSwitch

        [TestMethod]
        public void WorkspaceSessionPersistanceManagerTests_ContextSwitch_SwitchUrl_Success()
        {
            ValidateConnectionSwitch(() => OctaneConfiguration.Url = Guid.NewGuid().ToString());
        }

        [TestMethod]
        public void WorkspaceSessionPersistanceManagerTests_ContextSwitch_SwitchWorkspace_Success()
        {
            ValidateConnectionSwitch(() => OctaneConfiguration.WorkSpaceId = new Random().Next());
        }

        [TestMethod]
        public void WorkspaceSessionPersistanceManagerTests_ContextSwitch_SwitchSharedSpace_Success()
        {
            ValidateConnectionSwitch(() => OctaneConfiguration.SharedSpaceId = new Random().Next());
        }

        [TestMethod]
        public void WorkspaceSessionPersistanceManagerTests_ContextSwitch_SwitchUser_Success()
        {
            ValidateConnectionSwitch(() => OctaneConfiguration.Username = Guid.NewGuid().ToString());
        }

        private void ValidateConnectionSwitch(Action connectionSwitchAction)
        {
            OctaneConfiguration.Url = Guid.NewGuid().ToString();

            CollectionAssert.AreEqual(new List<string>(), WorkspaceSessionPersistanceManager.History, "Invalid initial history");

            WorkspaceSessionPersistanceManager.UpdateHistory("1");
            var defect = new Defect(111) { SubType = WorkItem.SUBTYPE_DEFECT };
            WorkspaceSessionPersistanceManager.SetActiveEntity(defect);

            connectionSwitchAction();

            CollectionAssert.AreEqual(new List<string>(), WorkspaceSessionPersistanceManager.History, "Mismatched history after reset");
            ValidateActiveEntity(null);

            WorkspaceSessionPersistanceManager.UpdateHistory("2");
            WorkspaceSessionPersistanceManager.SetActiveEntity(defect);

            CollectionAssert.AreEqual(new List<string> { "2" }, WorkspaceSessionPersistanceManager.History, "Mismatched history");
            ValidateActiveEntity(defect);
        }

        #endregion

        private void ValidateActiveEntity(BaseEntity expectedEntity)
        {
            if (expectedEntity == null)
            {
                Assert.IsNull(WorkspaceSessionPersistanceManager.GetActiveEntity(), "There shouldn't be any active entity");
                return;
            }
            var activeEntity = WorkspaceSessionPersistanceManager.GetActiveEntity();
            Assert.AreEqual(expectedEntity.Id, activeEntity.Id, "Mismatched entity ids");
            Assert.AreEqual(VisualStudio.Common.Utility.GetConcreteEntityType(expectedEntity),
                            VisualStudio.Common.Utility.GetConcreteEntityType(activeEntity),
                            "Mismatched entity types");

            Assert.IsTrue(WorkspaceSessionPersistanceManager.IsActiveEntity(expectedEntity), "Entity isn't the active entity");
        }
    }
}
