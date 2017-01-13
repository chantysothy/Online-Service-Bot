//using Microsoft.Bot.Builder.Dialogs;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Threading.Tasks;
//using OCSBot.Shared;
//using OCSBot.Shared.Models;
//using Microsoft.Bot.Connector;
//using Microsoft.Bot.Connector.DirectLine;
//using Newtonsoft.Json;
//using Microsoft.Bot.Builder.Luis.Models;
//using AgentBot.Localizations;
//using AgentBot.Configuration;

//namespace AgentBot.Dialogs
//{
//    [Serializable]
//    public class AgentReplyModeDialog : IDialog<string>
//    {
//        bool IsHumanInvestigating = false;
//        Dictionary<string, string> EndUser = null;
//        string _agentConversationId = null;
//        private void SetHumanInvestigating(bool isHumanInvestigating, string endUserId, string endUserName)
//        {
//            IsHumanInvestigating = isHumanInvestigating;
//            if (IsHumanInvestigating)
//                EndUser = new Dictionary<string, string> { { "Id", endUserId }, { "Name", endUserName } };//context.Activity.From;
//            else
//                EndUser = null;
//        }
//        public async Task StartAsync(IDialogContext context)
//        {
//            Logger.Info("In AgentReplyMode");
//            context.Wait(HumanInteraction);   
//        }
//        public async Task HumanInteraction(IDialogContext context, IAwaitable<Microsoft.Bot.Connector.IMessageActivity> item)
//        {
//            var humanInvestigated = await HumanProcess(context);
//            if (!humanInvestigated)
//            {
//                context.Done(Messages.BOT_CASE_CLOSED);
//            }
//        }
//        public async Task<bool> HumanProcess(IDialogContext context)
//        {
//            //Send messages from OCS User to Agent
//            var storage = new AgentStatusStorage(
//                        ConfigurationHelper.GetString("BotStatusDBConnectionString"));
//            var convRecord = (await storage.FindConversationActivityAsync(a => a.UserID == agent.Id)).FirstOrDefault();
//            Activity activity = (Activity)context.Activity;
//            var localResumptionCookie = UrlToken.Decode<ResumptionCookie>(convRecord.LocalActivity);
//            Logger.Info($"AgentBot::LocalResumptionCookie:{localResumptionCookie}");
//            var localActivity = localResumptionCookie.GetMessage();
//            var localReply = localActivity.CreateReply($"[{activity.From.Name}]{activity.Text}");
//            var localConnector = new ConnectorClient(new Uri(localActivity.ServiceUrl),
//                        new MicrosoftAppCredentials(
//                            ConfigurationHelper.GetString("MicrosoftAppId"),
//                            ConfigurationHelper.GetString("MicrosoftAppPassword")),
//                        true);
//            Microsoft.Bot.Connector.Conversations localConversation = new Microsoft.Bot.Connector.Conversations(localConnector);
//            localConversation.ReplyToActivity(localReply);
//            Logger.Info("done");

//            return true;
            
//        }
//    }
//}