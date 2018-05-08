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
using MicroFocus.Adm.Octane.Api.Core.Services;
using MicroFocus.Adm.Octane.Api.Core.Services.Query;
using MicroFocus.Adm.Octane.Api.Core.Services.RequestContext;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;

namespace MicroFocus.Adm.Octane.VisualStudio.Tests.Utilities
{
    /// <summary>
    /// Utility helper methods
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Wait until condition is satisfied.
        /// If it took more than the specified timeout, an assert will be triggered with the given message
        /// </summary>
        public static void WaitUntil(Func<bool> condition, string message, TimeSpan timeout)
        {
            WaitUntil(condition, message, timeout, new TimeSpan(0, 0, 0, 0, 50));
        }

        /// <summary>
        /// Wait until condition is satisfied.
        /// If it took more than the specified timeout, an assert will be triggered with the given message
        /// </summary>
        public static void WaitUntil(Func<bool> condition, string message, TimeSpan timeout, TimeSpan waitPeriodBetweenIterations)
        {
            var conditionSatisfied = SpinWait.SpinUntil(() =>
            {
                var safisfied = condition();
                if (!safisfied)
                {
                    Thread.Sleep(waitPeriodBetweenIterations);
                }
                return safisfied;
            }, timeout);

            if (!conditionSatisfied)
            {
                Assert.Fail(message);
            }
        }

        /// <summary>
        /// Get phase object with the given logical name for the specified entity type
        /// </summary>
        public static Phase GetPhaseForEntityByLogicalName(EntityService entityService, WorkspaceContext workspaceContext, string entityTypeName, string logicalName)
        {
            var queryPhrases = new List<QueryPhrase>
            {
                new LogicalQueryPhrase(Phase.ENTITY_FIELD, entityTypeName),
                new LogicalQueryPhrase(Phase.LOGICAL_NAME_FIELD, logicalName)
            };

            var result = entityService.Get<Phase>(workspaceContext, queryPhrases, null);
            Assert.AreEqual(1, result.total_count, $"There should only be one phase with the logical name \"{logicalName }\" for type \"{entityService}\"");
            return result.data[0];
        }

        private static WorkItemRoot _workItemRoot;

        /// <summary>
        /// Get the WorkItemRoot entity
        /// </summary>
        public static WorkItemRoot GetWorkItemRoot(EntityService entityService, WorkspaceContext workspaceContext)
        {
            if (_workItemRoot == null)
            {
                var fields = new List<String>() { Phase.NAME_FIELD };
                var result = entityService.Get<WorkItemRoot>(workspaceContext, null, fields);
                Assert.AreEqual(1, result.total_count, "There should only be one WorkItemRoot entity");
                _workItemRoot = result.data[0];
            }

            return _workItemRoot;
        }

        /// <summary>
        /// Deep clone given object
        /// </summary>
        public static T Clone<T>(T source)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));

            string serializedObject;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                serializer.WriteObject(memoryStream, source);

                memoryStream.Position = 0;
                using (StreamReader sr = new StreamReader(memoryStream))
                {
                    serializedObject = sr.ReadToEnd();
                }
            }

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(serializedObject)))
            {
                return (T)serializer.ReadObject(stream);
            }
        }
    }
}
