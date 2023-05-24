/*!
* Copyright 2017 – 2023 Open Text.
*
* The only warranties for products and services of Open Text and
* its affiliates and licensors (“Open Text”) are as may be set
* forth in the express warranty statements accompanying such products
* and services. Nothing herein should be construed as constituting an
* additional warranty. Open Text shall not be liable for technical or
* editorial errors or omissions contained herein. The information
* contained herein is subject to change without notice.
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
using System.Collections.Generic;

namespace MicroFocus.Adm.Octane.VisualStudio.Tests.Common
{
    /// <summary>
    /// Test class for <see cref="EntityComparerByLastModified"/>
    /// </summary>
    [TestClass]
    public class EntityComparerByLastModifiedTests
    {
        private readonly EntityComparerByLastModified _entityComparer = new EntityComparerByLastModified();

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

            Assert.AreEqual(1, _entityComparer.Compare(firstEntity, new BaseEntity()), "Mismatched compare result");
        }

        [TestMethod]
        public void EntityComparerByLastModifiedTests_Compare_OnlySecondEntityHasLastModifiedValue_Success()
        {
            var secondEntity = new BaseEntity();
            secondEntity.SetDateTimeValue(CommonFields.LastModified, DateTime.Now);

            Assert.AreEqual(-1, _entityComparer.Compare(new BaseEntity(), secondEntity), "Mismatched compare result");
        }

        [TestMethod]
        public void EntityComparerByLastModifiedTests_Compare_CompareEntityList_Success()
        {
            var date = DateTime.Now;
            var earlyEntity = new BaseEntity();
            earlyEntity.SetDateTimeValue(CommonFields.LastModified, date.AddHours(-1));
            var middleEntity = new BaseEntity();
            middleEntity.SetDateTimeValue(CommonFields.LastModified, date);
            var lateEntity = new BaseEntity();
            lateEntity.SetDateTimeValue(CommonFields.LastModified, date.AddHours(1));

            var result = new List<BaseEntity> { middleEntity, lateEntity, earlyEntity };
            result.Sort(_entityComparer);

            CollectionAssert.AreEqual(new List<BaseEntity> { lateEntity, middleEntity, earlyEntity }, result, "Mismatched sorted list");
        }
    }
}
