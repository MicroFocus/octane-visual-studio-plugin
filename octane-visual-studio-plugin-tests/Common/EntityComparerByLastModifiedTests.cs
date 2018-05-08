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
    /// Test class for <see cref="EntityComparerByLastModified"/>
    /// </summary>
    [TestClass]
    public class EntityComparerByLastModifiedTests
    {
        private EntityComparerByLastModified _entityComparer = new EntityComparerByLastModified();

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EntityComparerByLastModifiedTests_Compare_NullFirstEntity_Throws()
        {
            _entityComparer.Compare(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EntityComparerByLastModifiedTests_Compare_NullSecondEntity_Throws()
        {
            _entityComparer.Compare(new BaseEntity(), null);
        }

        [TestMethod]
        public void EntityComparerByLastModifiedTests_Compare_BothEntitiesHaveNoLastModifiedValues_Success()
        {
            Assert.AreEqual(0, _entityComparer.Compare(new BaseEntity(), new BaseEntity()), "Mismatched compare result");
        }

        [TestMethod]
        public void EntityComparerByLastModifiedTests_Compare_OnlyFirstEntityHasLastModifiedValue_Success()
        {
            var firstEntity = new BaseEntity();
            firstEntity.SetDateTimeValue(CommonFields.LastModified, DateTime.Now);

            Assert.AreEqual(0, _entityComparer.Compare(firstEntity, new BaseEntity()), "Mismatched compare result");
        }

        [TestMethod]
        public void EntityComparerByLastModifiedTests_Compare_OnlySecondEntityHasLastModifiedValue_Success()
        {
            var secondEntity = new BaseEntity();
            secondEntity.SetDateTimeValue(CommonFields.LastModified, DateTime.Now);

            Assert.AreEqual(-1, _entityComparer.Compare(new BaseEntity(), secondEntity), "Mismatched compare result");
        }
    }
}
