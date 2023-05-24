﻿/*!
* Copyright 2017 – 2023 Open Text.
*
* The only warranties for products and services of Open Text and
* its affiliates and licensors (“Open Text”) are as may be set
* forth in the express warranty statements accompanying such products
* and services. Nothing herein should be construed as constituting an
* additional warranty. Open Text shall not be liable for technical or
* editorial errors or omissions contained herein. The information
* contained herein is subject to change without notice.
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
using System.Threading.Tasks;

namespace MicroFocus.Adm.Octane.VisualStudio.Common
{
    /// <summary>
    /// Class for providing label metadata
    /// </summary>
    internal static class EntityLabelService
    {

        

        public static async Task<Dictionary<string, EntityLabelMetadata>> GetEntityLabelMetadataAsync()
        {
            try
            {
                OctaneServices _octaneService = OctaneServices.GetInstance();

                List<EntityLabelMetadata> entityLabelMetadatas = await _octaneService.GetEntityLabelMedata();
                Dictionary<string, EntityLabelMetadata> result = new Dictionary<string, EntityLabelMetadata>();
                foreach (EntityLabelMetadata elm in entityLabelMetadatas)
                {
                    if (elm.GetStringValue(EntityLabelMetadata.LANGUAGE).Equals("lang.en"))
                    {
                        result.Add(elm.GetStringValue(EntityLabelMetadata.ENTITY_TYPE_FIELD), elm);
                    }
                }

                return result;
            }
            catch (Exception)
            {
                // failed to obtain entity label metadata 
                // calling function knows how to handle null result
                return null;
            }
            
        }
        
    }
}