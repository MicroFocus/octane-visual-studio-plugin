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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MicroFocus.Adm.Octane.VisualStudio.Tests.Utilities.Entity
{
    /// <summary>
    /// Utility class for managing <see cref="Release"/> entities
    /// </summary>
    public static class ReleaseUtilities
    {
        /// <summary>
        /// Create a new release entity
        /// </summary>
        public static Release CreateRelease()
        {
            var name = "Release_" + Guid.NewGuid();
            var release = new Release
            {
                Name = name,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(10),
                SprintDuration = 7
            };

            var createdRelease = BaseOctanePluginTest.EntityService.Create(BaseOctanePluginTest.WorkspaceContext, release, new[] { "name" });
            Assert.AreEqual(name, createdRelease.Name, "Mismatched name for newly created release");
            return createdRelease;
        }
    }
}
