using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AzureSearchImport
{
    class Program
    {
        static void Main(string[] args)
        {
            string searchServiceName = ConfigurationManager.AppSettings["SearchServiceName"];
            string adminApiKey = ConfigurationManager.AppSettings["SearchServiceAdminApiKey"];
            string index = ConfigurationManager.AppSettings["SearchServiceIndexName"];

            SearchServiceClient serviceClient = new SearchServiceClient(searchServiceName, new SearchCredentials(adminApiKey));

            ISearchIndexClient indexClient = serviceClient.Indexes.GetClient(index);
            var kb = ReadKB();

            SearchItem[] items = JsonConvert.DeserializeObject<SearchItem[]>(kb);
            List<IndexAction<SearchItem>> merges = new List<IndexAction<SearchItem>>();
            foreach(SearchItem item in items)
            {
                merges.Add(IndexAction.MergeOrUpload(item));
            }

            var batch = IndexBatch.New(merges.ToArray());

            try
            {
                indexClient.Documents.Index<SearchItem>(batch);
            }
            catch (IndexBatchException e)
            {
                // Sometimes when your Search service is under load, indexing will fail for some of the documents in
                // the batch. Depending on your application, you can take compensating actions like delaying and
                // retrying. For this simple demo, we just log the failed document keys and continue.
                Console.WriteLine(
                    "Failed to index some of the documents: {0}",
                    String.Join(", ", e.IndexingResults.Where(r => !r.Succeeded).Select(r => r.Key)));
            }

            Console.WriteLine("Waiting for documents to be indexed...\n");
            Thread.Sleep(2000);
        }

        static string ReadKB()
        {
            var file = System.IO.File.ReadAllText(".\\kb.json");
            return file;
        }
    }
}
