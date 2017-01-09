using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OCSBot.KB
{
    /*
        {
            "@search.score": 0.36829275,
            "id": "1",
            "query": "如何申請登入e動郵局系統？",
            "category": "郵局",
            "subcategory": "郵務",
            "answer": "（一） 客戶如已申請網路郵局，得以網路郵局之網路帳號、網路密碼及使用者代號登入e動郵局；如未申請網路郵局，以存簿儲金帳戶申請者須親自持國民身分證、儲金簿、原留印鑑至郵局(非通儲戶請至立帳局)辦理，以劃撥儲金帳戶申請者須親持收支詳情單、印鑑、國民身分證至付款局或票據交換局辦理。註：如客戶須於e動郵局使用「非約定轉帳」、「繳費(稅)」、「i郵箱繳費」及「無卡提款」交易，請另申請「e動郵局憑證」(以下簡稱憑證，限個人帳戶申請)。（二）客戶如有多個帳戶要使用e動郵局服務，須分別申請網路郵局網路帳號，並以個別網路帳號登入。但因一個身分證字號只限申請一張憑證，故憑證只要其中一個帳戶申請即可共用，且單一行動設備僅得載入單一憑證，無法多個憑證共用單一行動設備。登入e動郵局系統之程序：1.啟動e動郵局APP。2.輸入帳號類型(存簿或劃撥)、網路帳號、網路密碼、使者代號後，即登入系統。"
        }
     * */
     public class AzSearchResult
    {
        [JsonProperty("@odata.context")]
        public string Context { get; set; }
        public AzSearchResultItem [] value { get; set; }
    }
    public class AzSearchResultItem
    {
        [JsonProperty("@search.score")]
        public float Score { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("query")]
        public string Query { get; set; }

        [JsonProperty("answer")]
        public string Answer { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("subcategory")]
        public string Subcategory { get; set; }
    }
}