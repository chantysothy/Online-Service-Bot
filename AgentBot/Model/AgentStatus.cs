using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AgentBot.Model
{
    [Serializable]
    public class AgentStatus
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ConversationId { get; set; }
        public DateTime AbsoluteExpireTimeUTC { get; set; } = new DateTime(2000, 12, 31);
        public bool IsLoggedIn { get; set; }
    }
}