using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCSBot.Shared.Models
{
    [Serializable]
    public class DirectLineChannelData
    {
        public int RoundTrip { get; set; }
        public string ConversationId { get; set; }
        public string UserID { get; set; }
        public string UserName { get; set; }
        public string DirectLineBotID { get; set; }
    }
}
