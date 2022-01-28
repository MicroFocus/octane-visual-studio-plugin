/*!
* © Copyright 2017-2022 Micro Focus or one of its affiliates.
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
using System.Linq;
using System.Threading.Tasks;

namespace MicroFocus.Adm.Octane.VisualStudio.Common
{
    /// <summary>
    /// Class for providing field metadata based on the entity type
    /// </summary>
    internal static class FieldsMetadataService
    {
        /// <summary>
        /// Converters for formatting information based on the metadata type
        /// </summary>
        private static readonly Dictionary<string, Func<object, object>> Converter = new Dictionary<string, Func<object, object>>();

        static FieldsMetadataService()
        {
            Converter.Add("date_time", value => DateTime.Parse(value.ToString()).ToString("MM/dd/yyyy HH:mm:ss"));
        }

        /// <summary>
        /// Return the list of field metadata for the given entity type
        /// </summary>
        internal static async Task<List<FieldMetadata>> GetFieldMetadata(BaseEntity entity)
        {
            if (entity == null)
                return new List<FieldMetadata>();

            OctaneServices octaneService = OctaneServices.GetInstance();
        
            var entityType = Utility.GetConcreteEntityType(entity);
            var fieldsMetadata = await octaneService.GetFieldsMetadata(entityType);
            List<FieldMetadata> fields = fieldsMetadata.Where(fm => fm.VisibleInUI).ToList();
            
            return fields;
        }

        /// <summary>
        /// Return the formatted value for the given entity field using the property datatype
        /// </summary>
        internal static object GetFormattedValue(BaseEntity entity, String fieldName, FieldMetadata fieldMetadata)
        {
            if (entity == null || string.IsNullOrEmpty(fieldName))
                return null;
            try
            {
                if (fieldMetadata == null)
                    return null;

                Func<object, object> func;
                if (!Converter.TryGetValue(fieldMetadata.FieldType, out func))
                    return null;

                var value = entity.GetValue(fieldName);
                DateTime datetime;
                string dateString = (String)value;
                if (fieldMetadata.GetStringValue("editable") == "True" && DateTime.TryParseExact(dateString, "yyyy-MM-ddTHH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeLocal, out datetime))
                {
                    return value;
                }
                return func(value);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
