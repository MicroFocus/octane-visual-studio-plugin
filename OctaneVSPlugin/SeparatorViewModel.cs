using System.Collections.Generic;

namespace MicroFocus.Adm.Octane.VisualStudio
{
    /// <summary>
    /// Separator ViewModel for representing separators between fields on WorkItem.
    /// </summary>
    public class SeparatorViewModel
    {
        public static IEnumerable<SeparatorViewModel> Generator()
        {
            while (true)
            {
                yield return new SeparatorViewModel();
            }
        }

        internal static SeparatorViewModel Make()
        {
            return new SeparatorViewModel();
        }
    }
}
