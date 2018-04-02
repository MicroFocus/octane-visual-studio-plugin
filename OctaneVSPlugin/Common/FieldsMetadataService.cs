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

        /// <summary>
        /// 
        /// </summary>
        private static readonly Dictionary<Tuple<string, string>, FieldMetadata> MetadataCache = new Dictionary<Tuple<string, string>, FieldMetadata>();

        /// <summary>
        /// Dictionary containing the field metadata for each entity type
        /// </summary>
        private static readonly Dictionary<string, List<FieldMetadata>> FieldMetadataCache = new Dictionary<string, List<FieldMetadata>>();

        private static string _currentOctaneUrl;
        private static OctaneServices _octaneService;

        static FieldsMetadataService()
        {
            Converter.Add("date_time", value => DateTime.Parse(value.ToString()).ToString("MM/dd/yyyy HH:mm:ss"));
        }

        internal static async Task<List<FieldMetadata>> GetFieldMetadata(string entityType)
        {
            if (_currentOctaneUrl != OctaneConfiguration.Url)
            {
                _octaneService = new OctaneServices(
                    OctaneConfiguration.Url,
                    OctaneConfiguration.SharedSpaceId,
                    OctaneConfiguration.WorkSpaceId,
                    OctaneConfiguration.Username,
                    OctaneConfiguration.Password);

                await _octaneService.Connect();

                MetadataCache.Clear();
                FieldMetadataCache.Clear();

                _currentOctaneUrl = OctaneConfiguration.Url;
            }

            List<FieldMetadata> fields;
            if (!FieldMetadataCache.TryGetValue(entityType, out fields))
            {
                var fieldsMetadata = await _octaneService.GetFieldsMetadata(entityType);
                fields = fieldsMetadata.Where(fm => fm.visible_in_ui).ToList();

                RegisterFieldMetadataForEntityType(entityType, fields);
            }

            return fields;
        }

        private static void RegisterFieldMetadataForEntityType(string entityType,
            List<FieldMetadata> entityFieldMetadata)
        {
            FieldMetadataCache[entityType] = entityFieldMetadata;
            foreach (var fieldMetadata in entityFieldMetadata)
            {
                MetadataCache[new Tuple<string, string>(entityType, fieldMetadata.name)] = fieldMetadata;
            }
        }

        internal static object GetFormattedValue(BaseEntity entity, string propertyName)
        {
            try
            {
                var entityType = Utility.GetConcreteEntityType(entity);
                FieldMetadata fieldMetadata;
                if (!MetadataCache.TryGetValue(new Tuple<string, string>(entityType, propertyName), out fieldMetadata))
                    return null;

                Func<object, object> func;
                if (!Converter.TryGetValue(fieldMetadata.field_type, out func))
                    return null;

                var value = entity.GetValue(propertyName);
                return func(value);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
