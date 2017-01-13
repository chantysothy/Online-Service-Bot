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
    public class ConversationManager
    {
        private readonly string CONVERSATION_TABLE_NAME = "conversationrecords";
        private readonly string COVNERSATION_LOCK_NAME = "conversationlocks";
        private CloudStorageAccount StorageAccount = null;

        public ConversationManager(string storageConnectionString)
        {
            StorageAccount = CloudStorageAccount.Parse(storageConnectionString);
        }
        public async Task<bool> UpdateConversationActivityAsync(ConversationRecord record)
        {
            var tableClient = StorageAccount.CreateCloudTableClient();
            var tableRef = tableClient.GetTableReference(CONVERSATION_TABLE_NAME);
            await tableRef.CreateIfNotExistsAsync();
            var blobClient = StorageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(COVNERSATION_LOCK_NAME);
            blobContainer.CreateIfNotExists();

            var lockBlob = blobContainer.GetBlockBlobReference($"{record.UserID}.lock");
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
                    var tableOperation = TableOperation.InsertOrMerge(record);
                    var result = tableRef.Execute(tableOperation);

                    var newRecord = tableRef.Execute(TableOperation.Retrieve<DynamicTableEntity>
                            (
                                record.PartitionKey,
                                record.RowKey
                            ));

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

        public async Task<ConversationRecord[]> FindConversationActivityAsync(Func<ConversationRecord,bool> func)
        {
            var tableClient = StorageAccount.CreateCloudTableClient();
            var tableRef = tableClient.GetTableReference(CONVERSATION_TABLE_NAME);
            await tableRef.CreateIfNotExistsAsync();
            try
            {
                try
                {
                    var query = tableRef.CreateQuery<ConversationRecord>().Where(func);

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

        public async Task<ConversationRecord[]> FindMyConversationActivityAsync(string myLocalUserID)
        {
            return await FindConversationActivityAsync(r => r.UserID.ToLower().CompareTo(myLocalUserID.ToLower()) == 0);
        }
    }
}
