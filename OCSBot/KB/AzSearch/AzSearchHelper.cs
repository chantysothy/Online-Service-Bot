using Newtonsoft.Json;
using OCSBot.Configuration;
using OCSBot.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OCSBot.KB
{
    public class AzSearchHelper
    {
        public static AzSearchResult SearchKB(string query)
        {
            //&searchMode=any&searchFields=query,category,subcategory
            //GET https://ocsdemokb.search.windows.net/indexes/postkbindex/docs?api-version=2016-09-01&search=*
            //api - key: [admin key]
            //2015-02-28-Preview
            try
            {
                query = HttpUtility.UrlEncode(query);
                var SearchUrl = ConfigurationHelper.GetString("AzSearch_Url") + "&search=" + query + "&searchMode=any&searchFields=query,category,subcategory";
                var SearchAdminKey = ConfigurationHelper.GetString("AzSearch_AdminKey");
                var http = new HttpHelper("application/json", SearchUrl);

                var result = http.GET(new Dictionary<string, string> { { "api-key", SearchAdminKey } });
                AzSearchResult results = JsonConvert.DeserializeObject<AzSearchResult>(result);
                return results;
            }
            catch (Exception exp)
            {
                //TODO:Log and return error
                throw;
            }
        }
    }
}