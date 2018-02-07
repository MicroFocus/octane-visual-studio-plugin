using MicroFocus.Adm.Octane.Api.Core.Entities;
using System;
using System.Collections.Generic;

namespace Hpe.Nga.Octane.VisualStudio
{
    /// <summary>
    /// Comparer for BaseEntity by LastModified field.
    /// </summary>
    internal class EntityComparerByLastModified : IComparer<BaseEntity>
    {
        // For now we always order in descending mode.
        private const bool isDescending = true;

        public int Compare(BaseEntity x, BaseEntity y)
        {
            DateTime? xTime = x.GetDateTimeValue(CommonFields.LAST_MODIFIED);
            DateTime? yTime = y.GetDateTimeValue(CommonFields.LAST_MODIFIED);
            int result;

            if (!xTime.HasValue && !yTime.HasValue)
            {
                result = 0;
            }
            else if (!xTime.HasValue)
            {
                result = 1;
            }
            else if (!yTime.HasValue)
            {
                result = 0;
            }
            else
            {
                result = xTime.Value.CompareTo(yTime.Value);
            }

            if (isDescending)
            {
                result = -1 * result;
            }

            return result;
        }
    }
}
