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
using System.Threading.Tasks;

namespace MicroFocus.Adm.Octane.VisualStudio.Common
{
    class MyWorkUtils
    {
        
        private static void verifyUserItem(BaseEntity baseEntity)
        {
            if ("user_item".Equals(baseEntity.TypeName))
            {
                throw new Exception("Given param entity is not of type: user_item, type is: " + baseEntity.TypeName);
            }
        }

        private async static Task<UserItem> CreateNewUserItem(BaseEntity wrappedBaseEntity)
        {
            UserItem newUserItem = new UserItem();
            newUserItem.SetLongValue("origin", 1L);
            // dummy value in order to be able to serialise the useritem object
            newUserItem.Id = 0;
            newUserItem.SetValue("is_new", true);
            newUserItem.SetValue("reason", null);
            newUserItem.SetValue("entity_type", wrappedBaseEntity.TypeName);

            WorkspaceUser workspaceUser = await OctaneServices.GetInstance().GetCurrentUser();
            newUserItem.SetValue(UserItem.USER_FIELD, workspaceUser);

            String followField = "my_follow_items_" + wrappedBaseEntity.TypeName;
            newUserItem.SetValue(followField, wrappedBaseEntity);

            return newUserItem;
        }

        public async static void AddToMyWork(BaseEntity baseEntity)
        {
            UserItem newUserItem = await CreateNewUserItem(baseEntity);

            OctaneServices octaneService = OctaneServices.GetInstance();
            octaneService.AddToMyWork(newUserItem);
        }

        public static async void RemoveFromMyWork(BaseEntity baseEntity)
        {
            OctaneServices octaneService = OctaneServices.GetInstance();
            UserItem userItem = await octaneService.FindUserItemForEntity(baseEntity);
            if (userItem == null)
            {
                return;
            }

            octaneService.RemoveFromMyWork(userItem);
           
        }
    }
}
