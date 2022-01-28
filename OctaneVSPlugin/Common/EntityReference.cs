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

namespace MicroFocus.Adm.Octane.VisualStudio.Common
{
    /// <summary>
    /// Used to retrieve information about specific fields 
    /// </summary>
    public class ReferenceEntityUtil
    {
        //Dictionary for every type, with value the ApiEntityName
        private static readonly Dictionary<string, string> ApiEntityNameTypePairs = new Dictionary<string, string>
        {
            {"work_item", "work_items" },
            {"story", "work_items" },
            {"quality_story", "work_items" },
            {"defect", "work_items" },
            {"work_item_root", "work_items" },
            {"epic", "work_items" },
            {"feature", "work_items" },

            {"test", "tests" },
            {"test_manual", "tests" },
            {"gherkin_test", "tests" },
            {"test_automated", "tests" },
            {"test_suite", "tests" },
            {"scenario_test", "tests" },

            {"task", "tasks" },
            {"phase", "phases" },
            {"transition", "transitions" },
            {"run", "runs" },
            {"run_manual", "runs" },
            {"run_suite", "runs" },
            {"run_automated", "runs" },
            {"gherkin_automated_run", "runs" },

            {"comment", "comments" },

            {"workspace_user", "workspace_users" },
            {"team", "teams" },

            {"requirement", "requirements" },
            {"requirement_document", "requirements" },
            {"requirement_folder", "requirements" },

            {"user_item", "user_items" },
            {"user_tag", "user_tags" },

            {"list_node", "list_nodes" },

            {"release", "releases" },

            {"sprint", "sprints" },

            {"milestone", "milestones" },

            {"product_area", "product_areas" },

            {"taxonomy_node", "taxonomy_nodes" },
            {"taxonomy_item_node", "taxonomy_nodes" }
        };
       
        public static string getEntityType(BaseEntity entity)
        {
            if (entity.GetValue("subtype") != null)
            {
                string subtype = entity.GetValue("subtype").ToString();

                //try finding the subtype
                if (subtype != null)
                {
                    return ApiEntityNameTypePairs[subtype];
                }
            }

            if (entity.GetValue("type") != null)
            {
                string type = entity.GetValue("type").ToString();

                //try finding the subtype
                if (type != null)
                {
                    return ApiEntityNameTypePairs[type];
                }
            }

            return null;
        }

        public static string getApiEntityName(string type)
        {
            try
            {
                return ApiEntityNameTypePairs[type];
            }
            catch (Exception)
            {
                throw new Exception("Reference entity type not supported: " + type);
            }
        }

       
    }
}
