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
using System.Collections.Generic;

namespace MicroFocus.Adm.Octane.VisualStudio.Common
{
    /// <summary>
    /// Container for entity type display names
    /// </summary>
    public static class EntityNames
    {
        private static readonly Dictionary<string, string> DisplayNames = new Dictionary<string, string>
        {
            // work item
            { WorkItem.SUBTYPE_STORY, "User Story" },
            { WorkItem.SUBTYPE_QUALITY_STORY, "Quality Story" },

            // test
            { Test.SUBTYPE_MANUAL_TEST, "Manual Test" },
            { TestGherkin.SUBTYPE_GHERKIN_TEST, "Gherkin Test" },
            { TestAutomated.SUBTYPE_TEST_AUTOMATED, "Automated Test" },
            { TestSuite.SUBTYPE_TEST_SUITE, "Test Suite" },

            // run
            { RunManual.SUBTYPE_RUN_MANUAL, "Manual Test Run" },
            { RunAutomated.SUBTYPE_RUN_AUTOMATED, "Automated Test Run" },
            { RunSuite.SUBTYPE_RUN_SUITE, "Test Suite Run" },

            // requirement
            { Requirement.SUBTYPE_DOCUMENT, "Requirement" }
        };

        private static readonly Dictionary<string, string> Initials = new Dictionary<string, string>
        {
            // work item
            { WorkItem.SUBTYPE_DEFECT, "D" },
            { WorkItem.SUBTYPE_STORY, "US" },
            { WorkItem.SUBTYPE_QUALITY_STORY, "QS" },

            // task
            { Task.TYPE_TASK, "T" },

            // test
            { Test.SUBTYPE_MANUAL_TEST, "MT" },
            { TestGherkin.SUBTYPE_GHERKIN_TEST, "GT" },
            { TestAutomated.SUBTYPE_TEST_AUTOMATED, "AT" },
            { TestSuite.SUBTYPE_TEST_SUITE, "TS" },

            // run
            { RunManual.SUBTYPE_RUN_MANUAL, "MR" },
            { RunAutomated.SUBTYPE_RUN_AUTOMATED, "AR" },
            { RunSuite.SUBTYPE_RUN_SUITE, "SR" },

            // requirement
            { Requirement.SUBTYPE_DOCUMENT, "R" },

            // comment
            { "comment", "C" }
        };

        /// <summary>
        /// Returns the display name for the given type.
        /// If the type isn't recognized, the input is returned.
        /// </summary>
        public static string GetDisplayName(string type)
        {
            return DisplayNames.TryGetValue(type, out var displayName) ? displayName : type;
        }

        /// <summary>
        /// Get initials for given entity type
        /// </summary>
        public static string GetInitials(string type)
        {
            return Initials.TryGetValue(type, out var displayName) ? displayName : type;
        }
    }
}
