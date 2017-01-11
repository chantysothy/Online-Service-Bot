using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCSBot.Shared.Models
{
    [Serializable]
    public class ConversationStatus: TableEntity
    {
        private string _ConversationId = null;
        private string _AgentId = null;
        public string AgentID
        {
            get { return _AgentId; }
            set
            {
                _AgentId = value;
                PartitionKey = value;
            }
        }
        public string ConversationId
        {
            get
            {
                return _ConversationId;
            }
            set
            {
                _ConversationId = value;
                RowKey = _ConversationId;

            }
        }
        public DateTime? ConversationStartUTCTime { get; set; } = new DateTime(1970,1,1);
        public string OCSEndUserId { get; set; }
        public string OCSEndUserName { get; set; }
        public string OCSBotId { get; set; }
        public string OCSBotName { get; set; }
        public string OCSDirectlineConversationId { get; set; }
        public string DirectlineBotId { get; set; }
        public string DirectlineBotName { get; set; }

        public string AgentResumptionCookie { get; set; }
    }
}
