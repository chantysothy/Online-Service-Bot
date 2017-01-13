using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCSBot.Shared.Models
{
    [Serializable]
    public class ConversationRecord:TableEntity
    {
        /// <summary>
        /// User Id of current bot channel
        /// </summary>
        private string _UserId = null;
        public string UserID
        {
            get { return _UserId; }
            set
            {
                _UserId = value;
                PartitionKey = value;
            }
        }

        private string _LocalConversationID = null;
        /// <summary>
        /// ConversationID of current bot channel
        /// </summary>
        public string LocalConversationID
        {
            get
            {
                return _LocalConversationID;
            }
            set
            {
                _LocalConversationID = value;
                RowKey = value;
            }
        }
        public string LocalBotId { get; set; }
        public string RemoteBotId { get; set; }
        public string RemoteUserId { get; set; }
        public string RemoteUserName { get; set; }
        /// <summary>
        /// Activity of current conversation, this activity will be used by the Bot server to send messages coming from remote to local user.
        /// Should be an UrlToken Encoded ResumptionCookie or other string format that can be deserialized to resume conversation.
        /// </summary>
        public string LocalActivity
        {
            get;set;
        }
        
        /// <summary>
        /// Remote Activitiy is used by Bot Server to communicate with remote Bot.
        /// Usually DirectLine activity
        /// </summary>
        public string RemoteActivity
        { get; set; }

        public DateTime? ConversationStartUTCTime { get; set; } = new DateTime(1970, 1, 1);
        public string LocalUserName { get; set; }
        public string LocalChannelID { get; set; }
        public string RemoteChannelID { get; set; }
        public string RemoteConversationID { get; set; }
    }
}
