using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using OCSBot.KB;
using OCSBot.Localizations;
using OCSBot.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace OCSBot.Dialogs
{   [Serializable]
    [LuisModel("3e1e4893-6c42-4aa0-b046-87b4ddd89f36",
                "6d35713e91ae40859618c38a6ceb95c5",
        LuisApiVersion.V2)]
    public partial class LuisDialogBox:LuisDialog<string>
    {
        private ResumptionCookie resumptionCookie = null;
        private Dictionary<string, string> propertyBag = new Dictionary<string, string>();
        
        protected async override Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            //whenever receives messages from the user, set resumptionCookie
            IMessageActivity activity = await item;
            resumptionCookie = new ResumptionCookie(activity);

            await base.MessageReceived(context, item);

            return;
        }
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            //When there is no intents met, query KB
            var kbResult = KBSearch.Search(result.Query);
            if(kbResult != null && kbResult.Count() > 0)
            {
                var winner = kbResult.OrderBy(o => o.userfeedback).First();
                await context.PostAsync(winner.content);
            }
            else
            {
                var answer = Messages.BOT_NO_ANSWERS;
                await context.PostAsync(answer);

                //TODO: redirect to human
                answer = Messages.BOT_CREATE_TICKET;
                await context.PostAsync(answer);
                Logger.Info($"Intent::None():{context.Activity.From.Name}/{context.Activity.Recipient.Name}");
                //IsHumanInvestigating = await HumanProcess(context, result);
                await HumanProcess(context, result);
            }
        }
    }
}