using Newtonsoft.Json;
using OCSBot.Configuration;
using OCSBot.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace OCSBot.KB
{
    public class QnAMakerHelper
    {
        //POST /knowledgebases/8ddadcb6-716d-4304-b296-c84a21e2f44f/generateAnswer
        //Host: https://westus.api.cognitive.microsoft.com/qnamaker/v1.0
        //Ocp-Apim-Subscription-Key: 66bb6fca81f64e41b25923d491473221
        //Content-Type: application/json
        //{"question":"hi"};
        public static QnAMakerResult SearchKB(string query)
        {
            //result:
            //{ "Answer": "Sample response", "Score": "0" }
            var QnAMaker_URL = ConfigurationHelper.GetString("QnAMaker_URL");
            var QnAMaker_Key = ConfigurationHelper.GetString("QnAMaker_Key");
            var http = new HttpHelper("application/json",QnAMaker_URL);
            var queryBody = JsonConvert.SerializeObject(new { question = query });
            var result = http.POST(queryBody, new Dictionary<string, string> { { "Ocp-Apim-Subscription-Key", QnAMaker_Key } });
            var resultObject = JsonConvert.DeserializeObject<QnAMakerResult>(result);
            return resultObject;
        }
    }
}