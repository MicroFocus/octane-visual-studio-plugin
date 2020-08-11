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

using MicroFocus.Adm.Octane.Api.Core.Connector;
using MicroFocus.Adm.Octane.Api.Core.Connector.Authentication;
using MicroFocus.Adm.Octane.Api.Core.Entities;
using MicroFocus.Adm.Octane.Api.Core.Entities.Base;
using MicroFocus.Adm.Octane.Api.Core.Services;
using MicroFocus.Adm.Octane.Api.Core.Services.Query;
using MicroFocus.Adm.Octane.Api.Core.Services.RequestContext;
using MicroFocus.Adm.Octane.Api.Core.Services.Version;
using MicroFocus.Adm.Octane.VisualStudio.Common;
using MicroFocus.Adm.Octane.VisualStudio.Common.Collector;
using NSoup.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Task = System.Threading.Tasks.Task;

namespace MicroFocus.Adm.Octane.VisualStudio
{
    /// <summary>
    /// Responsible for interacting with Octane API.
    /// </summary>
    internal class OctaneServices
    {
        private RestConnector rest;

        private EntityService es;

        private AuthenticationStrategy authenticationStrategy;

        public EntityService GetEntityService
        {
            get { return es; }
        }

        private string url;
        private string user;
        private string password;
        private WorkspaceContext workspaceContext;
        private SharedSpaceContext sharedSpaceContext;

        WorkspaceUser currentUser;
        
        private static readonly EntityComparerByLastModified EntityComparer = new EntityComparerByLastModified();

        private static OctaneServices instance = null;

        private OctaneServices(string url, long sharedspaceId, long workspaceId)
        {
            this.url = url;

            // create the authentication strategy based on saved configurations
            if (OctaneConfiguration.CredentialLogin)
            {
                authenticationStrategy = new LwssoAuthenticationStrategy(new UserPassConnectionInfo(OctaneConfiguration.Username, OctaneConfiguration.Password));
            }
            else if (OctaneConfiguration.SsoLogin)
            {
                SsoAuthenticationStrategy ssoAuthenticationStrategy = new SsoAuthenticationStrategy();
                ssoAuthenticationStrategy.SetConnectionListener(new SsoConnectionListener());
                authenticationStrategy = ssoAuthenticationStrategy;
            }
            
            rest = new RestConnector();
            es = new EntityService(rest);

            workspaceContext = new WorkspaceContext(sharedspaceId, workspaceId);
            sharedSpaceContext = new SharedSpaceContext(sharedspaceId);
        }

        public static OctaneServices GetInstance()
        {
            if(instance == null)
            {
                throw new Exception("Object not created");
            }
            return instance;
        }

        public static void Create(string url, long sharedspaceId, long workspaceId)
        {
            if(instance != null)
            {
                throw new Exception("Object already created");
            }
            instance = new OctaneServices(url, sharedspaceId, workspaceId);
        }

        public static async Task<bool> Reset()
        {
            bool result = false;

            if(instance != null)
            {
                result = await instance.rest.DisconnectAsync();
                instance = null;
            }

            return result;
        }

        public async Task Connect()
        {
            if (!rest.IsConnected())
            {
                await rest.ConnectAsync(url, authenticationStrategy);
                user = await authenticationStrategy.GetWorkspaceUser();
            }
        }


        public async Task<WorkspaceUser> GetCurrentUser()
        {
            if (currentUser == null)
            {
                currentUser = await GetWorkspaceUser();
            }
            return currentUser;
        }


        private IList<QueryPhrase> ToQueryList(QueryPhrase query)
        {
            List<QueryPhrase> queries = new List<QueryPhrase>
            {
                query
            };
            return queries;
        }

        private IList<QueryPhrase> BuildUserItemCriteria(WorkspaceUser user)
        {
            List<QueryPhrase> queries = new List<QueryPhrase>
            {
                new CrossQueryPhrase("user", new LogicalQueryPhrase("id", user.Id)),
            };

            return queries;
        }

        private IList<QueryPhrase> BuildFindUserItemCriteria(BaseEntity user,BaseEntity baseEntity)
        {
            List<QueryPhrase> queries = new List<QueryPhrase>
            {
                new CrossQueryPhrase("my_follow_items_" + baseEntity.TypeName, new LogicalQueryPhrase("id", baseEntity.Id)),
                new CrossQueryPhrase("user", new LogicalQueryPhrase("id", user.Id))             
            };

            return queries;
        }

        private LogicalQueryPhrase BuildCriteria(List<UserItem> userItems, Func<UserItem, BaseEntity> getReferenceEntityFunc)
        {
            var idPhrase = new LogicalQueryPhrase("id");

            foreach (UserItem userItem in userItems)
            {
                BaseEntity referenceEntity = getReferenceEntityFunc(userItem);
                if (referenceEntity != null)
                {
                    idPhrase.AddExpression(referenceEntity.Id, ComparisonOperator.Equal);
                }
            }

            return idPhrase;
        }

        public async Task<IList<BaseEntity>> GetMyItems()
        {
            var owner = await GetWorkspaceUser();
            EntityListResult<UserItem> userItems = await es.GetAsync<UserItem>(workspaceContext,
                BuildUserItemCriteria(owner), BuildUserItemFields());

            var collector = new MyWorkEntitiesCollector(this, userItems, MyWorkMetadata.Instance);
            List<BaseEntity> result = await collector.GetAllEntities();
            result.Sort(EntityComparer);

            return result;
        }

        public async Task<List<UserItem>> FindUserItemForEntity(BaseEntity baseEntity)
        {
            var owner = await GetCurrentUser();
            EntityListResult<UserItem> userItems = await es.GetAsync<UserItem>(workspaceContext, 
                BuildFindUserItemCriteria(owner, baseEntity), BuildUserItemFields());

            if(userItems.data.Count > 0)
            {
				return userItems.data;
            }
            else
            {
                return new List<UserItem>();
            }
        }
        

        public async Task<IList<BaseEntity>> SearchEntities(string searchString, int limitPerType)
        {
            var collector = new SearchEntitiesCollector(this, searchString, limitPerType);
            List<BaseEntity> result = await collector.GetAllEntities();
            return result;
        }

        public Task<EntityListResult<TEntity>> SearchEntitiesByType<TEntity>(string searchString, int limit, string type)
            where TEntity : BaseEntity
        {
            return es.SearchAsync<TEntity>(workspaceContext, searchString, new List<string> { type }, limit);
        }

        private IList<QueryPhrase> BuildCommentsCriteria(WorkspaceUser user)
        {
            return new List<QueryPhrase>
            {
                new CrossQueryPhrase("mention_user", new LogicalQueryPhrase("id", user.Id))
            };
        }

        private IList<QueryPhrase> BuildListNodeCriteria(string listName)
        {
            return new List<QueryPhrase>
            {
                new CrossQueryPhrase("list_root", new LogicalQueryPhrase("logical_name", listName))
            };
        }

        private readonly List<string> commentFields = new List<string>
        {
            Comment.AUTHOR_FIELD,
            Comment.OWNER_WORK_FIELD,
            Comment.OWNER_TEST_FIELD,
            Comment.OWNER_RUN_FIELD,
            Comment.OWNER_REQUIREMENT_FIELD,
            Comment.CREATION_TIME_FIELD,
            Comment.TEXT_FIELD
        };

        public async Task<IList<BaseEntity>> GetMyCommentItems()
        {
            var owner = await GetWorkspaceUser();
            EntityListResult<Comment> comments = await es.GetAsync<Comment>(workspaceContext, BuildCommentsCriteria(owner), commentFields);
            return comments.BaseEntities.ToList();
        }

        private async Task<WorkspaceUser> GetWorkspaceUser()
        {
            QueryPhrase ownerQuery = new LogicalQueryPhrase("name", user);
            EntityListResult<WorkspaceUser> ownerQueryResult = await es.GetAsync<WorkspaceUser>(workspaceContext, ToQueryList(ownerQuery), null);
            var workspaceUser = ownerQueryResult.data.FirstOrDefault();
            if (workspaceUser == null)
                throw new Exception($"Unable to find a user with the name \"{user}\"");
            return workspaceUser;
        }

        /// <summary>
        /// Returns the entity with the given ID and the requested fields
        /// </summary>
        public async Task<BaseEntity> FindEntityAsync(BaseEntity entityModel, IList<string> fields)
        {
            var entity = await es.GetByIdAsync(workspaceContext, entityModel.Id, Utility.GetConcreteEntityType(entityModel), fields);
            return entity;
        }

        private readonly Dictionary<string, string> commentSupport = new Dictionary<string, string>
        {
            { "work_item", "owner_work_item" },
            { "test", "owner_test" },
            { "run", "owner_run" },
            { "requirement", "owner_requirement" }
        };

        /// <summary>
        /// Update the properties of the given entity
        /// </summary>
        public async Task<BaseEntity> UpdateEntityAsync(BaseEntity entity)
        {
            var updatedEntity = await es.UpdateAsync(workspaceContext, entity, Utility.GetConcreteEntityType(entity));
            return updatedEntity;
        }
        
        ///<summary>
        ///Adds a comment with specified parameters
        /// </summary>
        public async Task<Comment> CreateCommentAsync(Comment entity)
        {
            RestConnector.AwaitContinueOnCapturedContext = false;
            var createdEntity = await es.CreateAsync(workspaceContext, entity, commentFields);
            return createdEntity;
        }

        ///<summary>
        ///Adds an entity to my work
        /// </summary>
        public UserItem AddToMyWork(UserItem entity)
        {
            RestConnector.AwaitContinueOnCapturedContext = false;
            return es.Create(workspaceContext, entity, null);
        }

        ///<summary>
        ///Removes an entity from my work
        /// </summary>
        public void RemoveFromMyWork(UserItem entity)
        {
            RestConnector.AwaitContinueOnCapturedContext = false;
            es.DeleteById<UserItem>(workspaceContext, entity.Id);
        }

        ///<summary>
        ///Removes a comment from my work
        /// </summary>
        public async Task RemoveCommentFromMyWork(BaseEntity entity)
        {
            RestConnector.AwaitContinueOnCapturedContext = false;
            string putUrl = workspaceContext.GetPath() + "/comments/" + entity.Id + "/dismiss";
            putUrl = putUrl.Replace("api", "internal-api");
            ResponseWrapper response = await rest.ExecutePutAsync(putUrl, null, null).ConfigureAwait(RestConnector.AwaitContinueOnCapturedContext);
        }

        /// <summary>
        /// Retrieves a list of all the comments attached to the given entity
        /// </summary>
        public async Task<List<Comment>> GetAttachedCommentsToEntity(BaseEntity entity)
        {
            if (string.IsNullOrEmpty(entity.AggregateType))
                return new List<Comment>();

            if (!commentSupport.TryGetValue(entity.AggregateType, out var type))
                return new List<Comment>();

            var query = new List<QueryPhrase>
            {
                new CrossQueryPhrase(type, new LogicalQueryPhrase("id", entity.Id))
            };
            var comments = await es.GetAsync<Comment>(workspaceContext, query, commentFields);
            return comments?.data;
        }

        public Task<EntityListResult<TEntity>> FetchEntities<TEntity>(
            List<UserItem> userItems,
            Func<UserItem, BaseEntity> getReferenceEntityFunc,
            MyWorkMetadata itemFetchInfo)
            where TEntity : BaseEntity
        {
            LogicalQueryPhrase idCriteria = BuildCriteria(userItems, getReferenceEntityFunc);
            if (idCriteria.Expressions.Count == 0)
            {
                // There are no items to select, return empty list.
                return Task.FromResult(new EntityListResult<TEntity>());
            }

            Task<EntityListResult<TEntity>> fetchEntitiesTask = es.GetAsync<TEntity>(workspaceContext,
                ToQueryList(idCriteria),
                itemFetchInfo.FieldsForType<TEntity>());

            return fetchEntitiesTask;
        }

        public Task<TestScript> GetTestScript(EntityId id)
        {
            return es.GetTestScriptAsync(workspaceContext, id);
        }

        private List<string> BuildUserItemFields()
        {
            string[] fields = new[]
            {
                UserItem.ENTITY_TYPE_FIELD,
                UserItem.REASON_FIELD,
                UserItem.ORIGIN,
                UserItem.USER_FIELD,
                UserItem.WORK_ITEM_REFERENCE,
                UserItem.TEST_REFERENCE,
                UserItem.RUN_REFERENCE,
                UserItem.REQUIREMENT_REFERENCE,
                UserItem.TASK_REFERENCE
            };
            return fields.ToList();
        }

        /// <summary>
        /// Return the fields' metadata for the given entity type
        /// </summary>
        public async Task<List<FieldMetadata>> GetFieldsMetadata(string entityType)
        {
            var result = await es.GetFieldsMetadataAsync(workspaceContext, entityType);
            return result?.data;
        }

        /// <summary>
        /// Return the label metadata for the entities
        /// </summary>
        public async Task<List<EntityLabelMetadata>> GetEntityLabelMedata()
        {
            var result = await es.GetLabelMetadataAsync(workspaceContext);
            return result?.data;
        }


        /// <summary>
        /// Async operation for downloading the attachment at the url and store it locally at the given location
        /// </summary>
        public async Task DownloadAttachmentAsync(string relativeUrl, string destinationPath)
        {
            await es.DownloadAttachmentAsync(relativeUrl, destinationPath);
        }

        /// <summary>
        /// Validate given commit message
        /// </summary>
        public async Task<CommitPattern> ValidateCommitMessageAsync(string commitMessage)
        {
            return await es.ValidateCommitMessageAsync(workspaceContext, HttpUtility.UrlEncode(commitMessage, Encoding.UTF8));
        }

        /// <summary>
        /// Return all transitions for a given entity type
        /// </summary>
        public async Task<List<Transition>> GetTransitionsForEntityType(string entityType)
        {
            var result = await es.GetTransitionsForEntityType(workspaceContext, entityType);
            return result?.data;
        }

        /// <summary>
        /// Returns all reference fields values for a given entity tpye
        /// </summary>
        public EntityListResult<BaseEntity> GetEntitesReferenceFields(string entityType)
        {
            return es.GetAsyncReferenceFields(workspaceContext, entityType, null, null, 100).Result;
        }

        /// <summary>
        /// Returns all reference fields values for a given entity tpye
        /// </summary>
        public EntityListResult<BaseEntity> GetEntitesReferenceFields(string entityType, IList<QueryPhrase> queryPhrases, List<string> fields)
        {
            return es.GetAsyncReferenceFields(workspaceContext, entityType, queryPhrases, fields, 100).Result;
        }

        /// <summary>
        /// Returns all reference fields list node values for a given entity tpye
        /// </summary>
        public EntityListResult<BaseEntity> GetEntitesReferenceListNodes(string entityType, string listName)
        {
            return es.GetAsyncReferenceFields(workspaceContext, entityType, BuildListNodeCriteria(listName), null, 100).Result;
        }

        /// <summary>
        /// Returns the version of octane
        /// </summary>
        public async Task<OctaneVersion> GetOctaneVersion()
        {
            return await es.GetOctaneVersion();
        }
    }

}
