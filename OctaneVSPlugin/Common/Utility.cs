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
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

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

        public static string GetAuthorFullName(BaseEntity entity)
        {
            return (string)GetPropertyOfChildEntity(entity, Comment.AUTHOR_FIELD, BaseUserEntity.FULL_NAME_FIELD);
        }

        /// <summary>
        /// Strip outer html tags from the given text
        /// </summary>
        public static string StripHtml(string text)
        {
            if (text == null)
                return string.Empty;

            var doc = NSoup.Parse.Parser.Parse(text, OctaneConfiguration.Url);
            return doc.Text();
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

        /// <summary>
        /// Open in the system's default browser the given entity
        /// </summary>
        public static void OpenInBrowser(BaseEntity entity)
        {
            if (entity == null)
                return;

            var url = $"{OctaneConfiguration.Url}/ui/entity-navigation?p={OctaneConfiguration.SharedSpaceId}/{OctaneConfiguration.WorkSpaceId}&entityType={Utility.GetBaseEntityType(entity)}&id={entity.Id}";

            // Open the URL in the user's default browser.
            Process.Start(url);
        }

        #region Serialization

        /// <summary>
        /// Deserialize given json value; if something went wrong, return the default value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T DeserializeFromJson<T>(string json, T defaultValue)
        {
            try
            {
                var serializer = new DataContractJsonSerializer(typeof(T));
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                {
                    T result = (T)serializer.ReadObject(stream);
                    return result != null ? result : defaultValue;
                }
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Serialize given value to json
        /// </summary>
        public static string SerializeToJson<T>(T value)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            using (MemoryStream memoryStream = new MemoryStream())
            {
                serializer.WriteObject(memoryStream, value);

                memoryStream.Position = 0;
                using (StreamReader sr = new StreamReader(memoryStream))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        #endregion

        /// <summary>
        /// Return the parent entity of a task; null otherwise
        /// </summary>
        public static BaseEntity GetTaskParentEntity(BaseEntity entity)
        {
            if (entity == null)
                return null;

            if (entity.TypeName == Task.TYPE_TASK)
            {
                return (BaseEntity)entity.GetValue("story");
            }

            return null;
        }
    }
}
