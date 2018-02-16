using System.Collections.Generic;

namespace Hpe.Nga.Octane.VisualStudio.Common
{
    /// <summary>
    /// Container for entity type display names
    /// </summary>
    public static class EntityNames
    {
        private static readonly Dictionary<string, string> DisplayNames = new Dictionary<string, string>
        {
            // work item
            { "story", "User Story" },
            { "quality_story", "Quality Story" },

            // test
            { "test_manual", "Manual Test" },
            { "gherkin_test", "Gherkin Test" },
            { "test_automated", "Automated Test" },
            { "test_suite", "Test Suite" },

            // run
            { "run_manual", "Manual Test Run" },
            { "run_suite", "Test Suite Run" },

            // requirement
            { "requirement_document", "Requirement" }
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
