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

namespace MicroFocus.Adm.Octane.VisualStudio.Common
{
    /// <summary>
    /// Utility class
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Returns the child's property of the given entity
        /// </summary>
        /// <param name="entity">parent entity</param>
        /// <param name="child">child entity property name</param>
        /// <param name="property">property name to be retieved from the child entity</param>
        public static object GetPropertyOfChildEntity(BaseEntity entity, string child, string property)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (string.IsNullOrEmpty(child))
                throw new ArgumentNullException(nameof(child));

            if (string.IsNullOrEmpty(property))
                throw new ArgumentNullException(nameof(property));

            var value = entity.GetValue(child);
            var childEntity = value as BaseEntity;
            return childEntity?.GetValue(property);
        }

        /// <summary>
        /// Returns the base entity type
        /// Example: for a user story, it will return the base "work_item" type
        ///          for a task, it will return "task"
        /// </summary>
        public static string GetBaseEntityType(BaseEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.AggregateType ?? entity.TypeName;
        }

        /// <summary>
        /// Returns the concrete/subtype entity type
        /// For example for a user story, it will return 'story'
        /// </summary>
        public static string GetConcreteEntityType(BaseEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var subtype = entity.GetStringValue(WorkItem.SUBTYPE_FIELD);
            return !string.IsNullOrEmpty(subtype) ? subtype : entity.TypeName;
        }
    }
}
