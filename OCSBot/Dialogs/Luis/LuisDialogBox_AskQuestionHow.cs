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
        //[LuisModel("3e1e4893-6c42-4aa0-b046-87b4ddd89f36",
        //            "6d35713e91ae40859618c38a6ceb95c5",
        //    LuisApiVersion.V2)]
        [LuisModel("da5b1b5f-6d8d-4130-9f45-70ba384d8b9c",
                "6d35713e91ae40859618c38a6ceb95c5",
        LuisApiVersion.V2)]
    public partial class LuisDialogBox:LuisDialog<string>
    {
        private ResumptionCookie resumptionCookie = null;
        private Dictionary<string, string> propertyBag = new Dictionary<string, string>();

        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"None intent reached");
        }
        [LuisIntent("AskQuestionHow")]
        public async Task AskQuestionHow(IDialogContext context, LuisResult result)
        {
            var kbResult = KBSearch.Search(result.Query, result.Entities != null ? result.Entities.Select(e => e.Entity).ToArray() : null);
            if(kbResult != null && kbResult.Count() > 0)
            {
                var winner = kbResult.OrderByDescending(o => o.userfeedback).First();
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