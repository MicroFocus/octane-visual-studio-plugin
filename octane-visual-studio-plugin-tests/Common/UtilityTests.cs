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
            Assert.AreEqual("custom_subtype", actualType);
        }

        [TestMethod]
        public void UtilityTests_GetConcreteEntityType_NoSubType_Success()
        {
            var entity = new WorkItem();
            entity.SetValue(BaseEntity.TYPE_FIELD, "custom_type");

            var actualType = Utility.GetConcreteEntityType(entity);
            Assert.AreEqual("custom_type", actualType);
        }
    }
}
