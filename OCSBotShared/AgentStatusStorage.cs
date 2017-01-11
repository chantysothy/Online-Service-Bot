using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using OCSBot.Shared.Model;
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
        private readonly string CONTAINERNAME_AGNET_LEASE_LOCK = "agentlease";
        private CloudStorageAccount StorageAccount = null;
        
        public AgentStatusStorage(string connectString)
        {
            ConnectionString = connectString;
            StorageAccount = CloudStorageAccount.Parse(ConnectionString);
        }
        public async Task<AgentStatus> QueryAgentSigninStatusAsync(string agentId)
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
        public async Task<TableResult> UpdateAgentSigninStatusAsync(AgentStatus obj)
        {
            var tableClient = StorageAccount.CreateCloudTableClient();
            var tableRef = tableClient.GetTableReference(TABLENAME_AGENT_SIGNIN_STATUS);
            await tableRef.CreateIfNotExistsAsync();
            var blobClient = StorageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(CONTAINERNAME_AGNET_LEASE_LOCK);
            blobContainer.CreateIfNotExists();

            var lockBlob = blobContainer.GetBlockBlobReference($"{obj.Id}.lock");
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
                    //var entity = GetTableEntity(obj);
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

        public async Task<AgentStatus[]> FindAvailableAgentsAsync()
        {
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
                    //var query = new TableQuery<AgentStatus>().Where(
                    //    TableQuery.CombineFilters(
                    //          TableQuery.GenerateFilterCondition("IsOccupied", QueryComparisons.Equal, "False"),
                    //          TableOperators.And,
                    //          TableQuery.GenerateFilterCondition("IsLoggedIn", QueryComparisons.Equal, "True")
                    //          )
                    //        );
                    //var results = tableRef.ExecuteQuery<AgentStatus>(query).ToArray();

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
    }
}
