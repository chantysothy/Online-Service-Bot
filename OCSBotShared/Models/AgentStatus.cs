using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OCSBot.Shared.Models
{
    [Serializable]
    public class AgentStatus:TableEntity
    {
        private string _Office;
        private string _Id;
        public string Office {
            get
            {
                return _Office;
            }
            set
            {
                _Office = value;
                PartitionKey = value;
            }
        }
        public string Id {
            get
            {
                return _Id;
            }
            set
            {
                _Id = value;
                RowKey = value;
            }
        }
        public string BotNameInChannel { get; set; }
        public string BotIdInChannel { get; set; }
        public string ChannelId { get; set; }
        public string AgentIdInChannel { get; set; }
        public string AgentNameInChannel { get; set; }
        public DateTime? LastConversationStartTime { get; set; }
        public DateTime? LastConversationEndTime { get; set; }
        public DateTime? LoginTime { get; set; }
        public bool IsOccupied { get; set; }
        public string Name { get; set; }
        public string ConversationId { get; set; }
        public bool IsLoggedIn { get; set; }
        public string ServiceURL { get; set; }
    }
}