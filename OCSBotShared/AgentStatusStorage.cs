using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using OCSBot.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCSBot.Shared
{
    public class AgentStatusStorage
    {
        private string ConnectionString = null;
        private readonly string TABLENAME_AGENT_SIGNIN_STATUS = "agentsigninstatus";
        private readonly string TABLENAME_AGENT_CONVERSATION_STATUS = "agentconversationstatus";
        private readonly string CONTAINERNAME_AGNET_LEASE_LOCK = "agentlease";
        private CloudStorageAccount StorageAccount = null;
        private ConversationManager ConvManager = null;
        public AgentStatusStorage(string connectString)
        {
            ConnectionString = connectString;
            StorageAccount = CloudStorageAccount.Parse(ConnectionString);
            ConvManager = new ConversationManager(connectString);
        }
        public async Task<AgentStatus> QueryAgentStatusAsync(string agentId)
        {
            var tableClient = StorageAccount.CreateCloudTableClient();
            var tableRef = tableClient.GetTableReference(TABLENAME_AGENT_SIGNIN_STATUS);
            await tableRef.CreateIfNotExistsAsync();
            try
            {
                try
                {
                    var query = new TableQuery<AgentStatus>().Where(
                              TableQuery.GenerateFilterCondition(
                                "RowKey",
                                QueryComparisons.Equal,
                                agentId
                              )
                            );
                    var results = tableRef.ExecuteQuery<AgentStatus>(query).SingleOrDefault();

                    return results;
                }
                catch (Exception exp)
                {
                    throw exp;
                }
                finally
                {

                }
            }
            catch (Exception exp)
            {
                throw;
            }
            finally
            {
            }
        }
        /*
        public async Task<ConversationStatus[]> QueryConversationStatusAsync(Func<ConversationStatus, bool> func)
        {
            var tableClient = StorageAccount.CreateCloudTableClient();
            var tableRef = tableClient.GetTableReference(TABLENAME_AGENT_CONVERSATION_STATUS);
            await tableRef.CreateIfNotExistsAsync();
            try
            {
                try
                {
                    var query = tableRef.CreateQuery<ConversationStatus>().Where(func);

                    return query.ToArray();
                }
                catch (Exception exp)
                {
                    throw exp;
                }
                finally
                {

                }
            }
            catch (Exception exp)
            {
                throw;
            }
            finally
            {
            }
        }
        
        public async Task<ConversationStatus[]> QueryConversationStatusAsync(string agentId)
        {
            return await QueryConversationStatusAsync(x => x.AgentID == agentId);
        }
        */
        public async Task<TableResult> UpdateAgentStatusAsync(AgentStatus obj)
        {
            var tableClient = StorageAccount.CreateCloudTableClient();
            var tableRef = tableClient.GetTableReference(TABLENAME_AGENT_SIGNIN_STATUS);
            await tableRef.CreateIfNotExistsAsync();
            var blobClient = StorageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(CONTAINERNAME_AGNET_LEASE_LOCK);
            blobContainer.CreateIfNotExists();

            var lockBlob = blobContainer.GetBlockBlobReference($"{obj.ConversationId}.lock");
            if (!lockBlob.Exists())
            {
                lockBlob.UploadText("");
            }
            try
            {
                var leaseId = lockBlob.AcquireLease(
                    TimeSpan.FromSeconds(15),
                    null);
                try
                {
                    var tableOperation = TableOperation.InsertOrMerge(obj);
                    var result = tableRef.Execute(tableOperation);

                    var newRecord = tableRef.Execute(TableOperation.Retrieve<DynamicTableEntity>
                            (
                                obj.PartitionKey,
                                obj.RowKey
                            ));

                    return newRecord;
                }
                catch (Exception exp)
                {
                    throw;
                }
                finally
                {
                    lockBlob.ReleaseLease(AccessCondition.GenerateLeaseCondition(leaseId));
                }
            }
            catch (Exception exp)
            {
                throw;
            }
            finally
            {
            }
        }
        public async Task<AgentStatus []> FindAvailableAgentsAsync(Func<AgentStatus, bool> func)
        {
            var tableClient = StorageAccount.CreateCloudTableClient();
            var tableRef = tableClient.GetTableReference(TABLENAME_AGENT_SIGNIN_STATUS);
            await tableRef.CreateIfNotExistsAsync();
            try
            {
                try
                {
                    var query = tableRef.CreateQuery<AgentStatus>().Where(func);

                    return query.ToArray();
                }
                catch (Exception exp)
                {
                    throw exp;
                }
                finally
                {

                }
            }
            catch (Exception exp)
            {
                throw;
            }
            finally
            {
            }
        }
        public async Task<AgentStatus[]> FindAvailableAgentsAsync()
        {
            return await FindAvailableAgentsAsync(agent => agent.IsLoggedIn && !agent.IsOccupied);
            var tableClient = StorageAccount.CreateCloudTableClient();
            var tableRef = tableClient.GetTableReference(TABLENAME_AGENT_SIGNIN_STATUS);
            await tableRef.CreateIfNotExistsAsync();
            try
            {
                try
                {
                    var query = from agent in tableRef.CreateQuery<AgentStatus>()
                                where agent.IsLoggedIn && !agent.IsOccupied
                                select agent;
  
                    return query.ToArray();
                }
                catch (Exception exp)
                {
                    throw exp;
                }
                finally
                {

                }
            }
            catch (Exception exp)
            {
                throw;
            }
            finally
            {
            }
        }

        public async Task<bool> UpdateConversationStatusAsync(ConversationStatus obj)
        {
            var tableClient = StorageAccount.CreateCloudTableClient();
            var tableRef = tableClient.GetTableReference(TABLENAME_AGENT_CONVERSATION_STATUS);
            await tableRef.CreateIfNotExistsAsync();
            var blobClient = StorageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(CONTAINERNAME_AGNET_LEASE_LOCK);
            blobContainer.CreateIfNotExists();

            var lockBlob = blobContainer.GetBlockBlobReference($"conversation{obj.ConversationId}.lock");
            if (!lockBlob.Exists())
            {
                lockBlob.UploadText("");
            }
            try
            {
                var leaseId = lockBlob.AcquireLease(
                    TimeSpan.FromSeconds(15),
                    null);
                try
                {
                    var tableOperation = TableOperation.InsertOrMerge(obj);
                    var result = tableRef.Execute(tableOperation);

                    return true;
                }
                catch (Exception exp)
                {
                    throw;
                }
                finally
                {
                    lockBlob.ReleaseLease(AccessCondition.GenerateLeaseCondition(leaseId));
                }
            }
            catch (Exception exp)
            {
                throw;
            }
            finally
            {
            }
        }
        public async Task<bool> UpdateConversationActivityAsync(ConversationRecord record)
        {
            return await ConvManager.UpdateConversationActivityAsync(record);
        }
        public async Task<ConversationRecord[]> FindConversationActivityAsync(Func<ConversationRecord, bool> func)
        {
            return await ConvManager.FindConversationActivityAsync(func);
        }
        public async Task<ConversationRecord[]> FindMyConversationActivityAsync(string myLocalUserID)
        {
            return await ConvManager.FindMyConversationActivityAsync(myLocalUserID);
        }
    }
}
