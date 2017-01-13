using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;

namespace OCSBot.Dialogs
{
    [Serializable]
    public class AgentReplyDialog : IDialog<string>
    {
        public async Task StartAsync(IDialogContext context)
        {
            var incoming = context.Activity as Activity;

        }
    }
}