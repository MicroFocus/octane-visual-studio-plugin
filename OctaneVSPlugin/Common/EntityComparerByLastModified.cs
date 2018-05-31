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
using System;
using System.Collections.Generic;

namespace MicroFocus.Adm.Octane.VisualStudio.Common
{
    /// <summary>
    /// Comparer for BaseEntity by LastModified field (only in descending mode)
    /// </summary>
    internal class EntityComparerByLastModified : IComparer<BaseEntity>
    {
        /// <inheritdoc/>
        public int Compare(BaseEntity first, BaseEntity second)
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));
            if (second == null)
                throw new ArgumentNullException(nameof(second));

            DateTime? firstDateTime = first.GetDateTimeValue(CommonFields.LastModified);
            DateTime? secondDateTime = second.GetDateTimeValue(CommonFields.LastModified);

            if (!firstDateTime.HasValue && !secondDateTime.HasValue)
            {
                return 0;
            }
            if (!firstDateTime.HasValue)
            {
                return -1;
            }
            if (!secondDateTime.HasValue)
            {
                return 1;
            }

            return secondDateTime.Value.CompareTo(firstDateTime.Value);
        }
    }
}
