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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MicroFocus.Adm.Octane.VisualStudio.Tests.Common
{
    /// <summary>
    /// Test class for <see cref="Utility"/>
    /// </summary>
    [TestClass]
    public class UtilityTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UtilityTests_GetPropertyOfChildEntity_NullEntity_Throws()
        {
            Utility.GetPropertyOfChildEntity(null, null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UtilityTests_GetPropertyOfChildEntity_NullChild_Null()
        {
            var entity = new WorkItem();
            Utility.GetPropertyOfChildEntity(entity, null, null);
        }

        [TestMethod]
        public void UtilityTests_GetPropertyOfChildEntity_InvalidChild_Null()
        {
            var entity = new WorkItem();
            Assert.IsNull(Utility.GetPropertyOfChildEntity(entity, "invalid", "property"), "Invalid child should return null");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UtilityTests_GetPropertyOfChildEntity_NullProperty_Null()
        {
            var entity = new WorkItem();
            Utility.GetPropertyOfChildEntity(entity, "child", null);
        }

        [TestMethod]
        public void UtilityTests_GetPropertyOfChildEntity_ChildIsNotBaseEntity_Null()
        {
            var entity = new WorkItem();
            entity.SetIntValue("child", 1);
            Assert.IsNull(Utility.GetPropertyOfChildEntity(entity, "child", "property"), "Invalid child type should return null");
        }

        [TestMethod]
        public void UtilityTests_GetPropertyOfChildEntity_MissingProperty_Null()
        {
            var entity = new WorkItem();
            entity.SetValue("child", new WorkItem());
            Assert.IsNull(Utility.GetPropertyOfChildEntity(entity, "child", "invalid"), "Invalid property should return null");
        }

        [TestMethod]
        public void UtilityTests_GetPropertyOfChildEntity_PropertyExists_Success()
        {
            var child = new WorkItem();
            child.SetIntValue("property", 1);
            var entity = new WorkItem();
            entity.SetValue("child", child);
            Assert.AreEqual(1, (int)Utility.GetPropertyOfChildEntity(entity, "child", "property"), "Mismatched property values");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UtilityTests_GetBaseEntityType_NullEntity_Throws()
        {
            Utility.GetBaseEntityType(null);
        }

        [TestMethod]
        public void UtilityTests_GetBaseEntityType_TypeWithAggregateType_Success()
        {
            var entity = new WorkItem();
            entity.SetValue(BaseEntity.TYPE_FIELD, "custom_type");

            var actualType = Utility.GetBaseEntityType(entity);
            Assert.AreEqual(entity.AggregateType, actualType, "Mismatched entity base type");
        }

        [TestMethod]
        public void UtilityTests_GetBaseEntityType_NoAggregateType_Success()
        {
            var entity = new BaseEntity();
            entity.SetValue(BaseEntity.TYPE_FIELD, "custom_type");

            var actualType = Utility.GetBaseEntityType(entity);
            Assert.AreEqual("custom_type", actualType, "Mismatched entity type");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UtilityTests_GetConcreteEntityType_NullEntity_Throws()
        {
            Utility.GetConcreteEntityType(null);
        }

        [TestMethod]
        public void UtilityTests_GetConcreteEntityType_TypeWithSubType_Success()
        {
            var entity = new WorkItem();
            entity.SetValue(BaseEntity.TYPE_FIELD, "custom_type");
            entity.SubType = "custom_subtype";

            var actualType = Utility.GetConcreteEntityType(entity);
            Assert.AreEqual("custom_subtype", actualType, "Mismatched entity sub-type");
        }

        [TestMethod]
        public void UtilityTests_GetConcreteEntityType_NoSubType_Success()
        {
            var entity = new WorkItem();
            entity.SetValue(BaseEntity.TYPE_FIELD, "custom_type");

            var actualType = Utility.GetConcreteEntityType(entity);
            Assert.AreEqual("custom_type", actualType, "Mismatched entity type");
        }
    }
}
