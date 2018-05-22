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

namespace MicroFocus.Adm.Octane.VisualStudio.Tests.Utilities.Entity
{
    /// <summary>
    /// Utility class for managing <see cref="Feature"/> entities
    /// </summary>
    public static class FeatureUtilities
    {
        private static Phase _phaseNew;

        private static Phase GetPhaseNew()
        {
            if (_phaseNew == null)
            {
                _phaseNew = Utility.GetPhaseForEntityByLogicalName(WorkItem.SUBTYPE_FEATURE, "phase.feature.new");
            }

            return _phaseNew;
        }

        /// <summary>
        /// Create a new feature entity
        /// </summary>
        public static Feature CreateFeature(Epic parentEpic, string customName = null)
        {
            var featureName = customName ?? "Feature_" + Guid.NewGuid();
            var featureToCreate = new Feature
            {
                Name = featureName,
                Phase = GetPhaseNew(),
                Parent = parentEpic
            };
            featureToCreate.SetValue(CommonFields.Owner, BaseOctanePluginTest.User);

            var createdFeature = BaseOctanePluginTest.EntityService.Create(BaseOctanePluginTest.WorkspaceContext, featureToCreate, new[] { "name", "subtype" });
            Assert.AreEqual(featureName, createdFeature.Name, "Mismatched feature name");

            return createdFeature;
        }
    }
}
