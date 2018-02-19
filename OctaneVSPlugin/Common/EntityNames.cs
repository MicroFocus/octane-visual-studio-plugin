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

        /// <summary>
        /// Returns the display name for the given type.
        /// If the type isn't recognized, the input is returned.
        /// </summary>
        public static string GetDisplayName(string type)
        {
            return DisplayNames.TryGetValue(type, out var displayName) ? displayName : type;
        }
    }
}
