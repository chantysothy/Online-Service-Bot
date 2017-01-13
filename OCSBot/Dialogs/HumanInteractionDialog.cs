using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using OCSBot.Shared;
using OCSBot.Shared.Models;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.DirectLine;
using Newtonsoft.Json;
using Microsoft.Bot.Builder.Luis.Models;
using OCSBot.Localizations;

namespace OCSBot.Dialogs
{
    [Serializable]
    public class HumanInteractionDialog : IDialog<string>
    {
        bool IsHumanInvestigating = false;
        Dictionary<string, string> EndUser = null;
        string _agentConversationId = null;
        private void SetHumanInvestigating(bool isHumanInvestigating, string endUserId, string endUserName)
        {
            IsHumanInvestigating = isHumanInvestigating;
            if (IsHumanInvestigating)
                EndUser = new Dictionary<string, string> { { "Id", endUserId }, { "Name", endUserName } };//context.Activity.From;
            else
                EndUser = null;
        }
        public async Task StartAsync(IDialogContext context)
        {
            Logger.Info("In HumandDialog");
            context.Wait(HumanInteraction);
            //await HumanInteraction(context, (Microsoft.Bot.Builder.Dialogs.IAwaitable<Microsoft.Bot.Connector.IMessageActivity>)new AwaitableFromItem<Microsoft.Bot.Connector.IMessageActivity>((Microsoft.Bot.Connector.IMessageActivity)context.Activity));
        }
        public async Task HumanInteraction(IDialogContext context, IAwaitable<Microsoft.Bot.Connector.IMessageActivity> item)
        {
            var humanInvestigated = await HumanProcess(context);
            //if (!humanInvestigated)
            //{
            //    context.Done("很抱歉目前客服人員都在忙線中，我們已經為您建立了一個Ticket。");
            //}
            //TODO:remove below
            context.Done("temoeratory end dialog...");
        }
        public async Task<bool> HumanProcess(IDialogContext context)
        {
            //TODO:end human investigating process
            var statusDB = Configuration.ConfigurationHelper.GetString("BotStatusDBConnectionString");
            var db = new AgentStatusStorage(statusDB);
            var agents = await db.FindAvailableAgentsAsync();
            var agent = agents.FirstOrDefault();

            Logger.Info($"Human Investigating:{IsHumanInvestigating}");

            if (agent != null)
            {
                //from bot to agent
                //find agent's conversation record
                var agentConversation = (await db.FindMyConversationActivityAsync(agent.Id)).FirstOrDefault();
                var localConversation = (await db.FindMyConversationActivityAsync(context.Activity.From.Id)).FirstOrDefault();
                ResumptionCookie cookie = new ResumptionCookie((Microsoft.Bot.Connector.IMessageActivity)context.Activity);
                if (localConversation == null)
                {
                    localConversation = new ConversationRecord();
                }
                localConversation.UserID = context.Activity.From.Id;
                localConversation.LocalUserName = context.Activity.From.Name;
                localConversation.LocalBotId = context.Activity.Recipient.Id;
                localConversation.LocalChannelID = context.Activity.ChannelId;
                localConversation.LocalConversationID = context.Activity.Conversation.Id;
                localConversation.LocalActivity = UrlToken.Encode<ResumptionCookie>(cookie);

                var resp2 = await PostToAgentBotAsync(new Microsoft.Bot.Connector.DirectLine.Activity()
                {
                    Type = Microsoft.Bot.Connector.DirectLine.ActivityTypes.Message,
                    From = new Microsoft.Bot.Connector.DirectLine.ChannelAccount(
                                                /*id: agentConversation.LocalBotId,*/
                                                id: context.Activity.From.Id,
                                                name: context.Activity.From.Name
                                            ),
                    Recipient = new Microsoft.Bot.Connector.DirectLine.ChannelAccount(
                                                id: agentConversation.UserID),
                    ChannelId = agent.ChannelId,
                    Text = $"{context.Activity.AsMessageActivity().Text}"
                });
                await db.UpdateConversationActivityAsync(localConversation);

                
                SetHumanInvestigating(true, context.Activity.From.Id, context.Activity.From.Name);

                return true;

            }
            else
            {
                var connector = new ConnectorClient(new Uri(context.Activity.ServiceUrl));
                connector.Conversations.ReplyToActivity(context.Activity.Conversation.Id,
                                                            context.Activity.Id,
                                                            new Microsoft.Bot.Connector.Activity
                                                            {
                                                                Type = Microsoft.Bot.Connector.DirectLine.ActivityTypes.Message,
                                                                Text = Messages.BOT_NO_AGENTS_AVAILABLE,
                                                                From = context.Activity.Recipient,
                                                                Recipient = context.Activity.From,
                                                                ChannelId = context.Activity.ChannelId
                                                            });
                SetHumanInvestigating(false, null, null);
                return false;
            }

        }
        private async Task<Microsoft.Bot.Connector.DirectLine.Activity> PostToAgentBotAsync(Microsoft.Bot.Connector.DirectLine.Activity activityFromUser)
        {
            var directLineSecret = Configuration.ConfigurationHelper.GetString("AgentBot_DirectLine_Secret");
            var agentStatusDB = Configuration.ConfigurationHelper.GetString("BotStatusDBConnectionString");
            var dc = new DirectLineClient(directLineSecret);
            var agentStorage = new AgentStatusStorage(agentStatusDB);
            var agent = await agentStorage.QueryAgentStatusAsync(activityFromUser.Recipient.Id);
            ConversationStatus convStatus = null;
            var agentConversations = await agentStorage.QueryConversationStatusAsync(agent.Id);

            try
            {
                var uri = new Uri("https://directline.botframework.com");
                DirectLineClientCredentials creds = new DirectLineClientCredentials(directLineSecret); //lot into the bot framework
                DirectLineClient client = new DirectLineClient(uri, creds); //connect the client
                Microsoft.Bot.Connector.DirectLine.Conversations convs = new Microsoft.Bot.Connector.DirectLine.Conversations(client); //get the list of conversations belonging to the bot? Or does this start a new collection of conversations?

                Microsoft.Bot.Connector.DirectLine.Conversation conversation = null;
                if (string.IsNullOrEmpty(_agentConversationId))
                {
                    conversation = dc.Conversations.StartConversation();
                    _agentConversationId = conversation.ConversationId;
                }
                else
                {
                    conversation = new Microsoft.Bot.Connector.DirectLine.Conversation()
                    {
                        ConversationId = _agentConversationId,
                    };
                }
                Logger.Info($"activityFromUser - From.Name:{activityFromUser.From.Name} - From.Id:{activityFromUser.From.Id}");
                Logger.Info($"activityFromUser - Recipient.Name:{activityFromUser.Recipient.Name} - Recipient.Id:{activityFromUser.Recipient.Name}");
                var toAgent = new Microsoft.Bot.Connector.DirectLine.Activity
                {
                    Type = Microsoft.Bot.Connector.DirectLine.ActivityTypes.Message,
                    Text = activityFromUser.Text,
                    From = new Microsoft.Bot.Connector.DirectLine.ChannelAccount
                    {
                        Id = activityFromUser.From.Id,/*activityFromUser.From.Id,*/
                        Name = $"{activityFromUser.From.Name}@ocsuser"
                    },
                    Recipient = activityFromUser.Recipient,
                    ChannelId = agent.ChannelId,
                    ChannelData = new DirectLineChannelData
                    {
                        RoundTrip = 0,
                        ConversationId = _agentConversationId,
                        UserID = activityFromUser.From.Id,
                        UserName = activityFromUser.From.Name
                    }
                };

                var resp = await dc.Conversations.PostActivityAsync(
                                                    conversation.ConversationId,
                                                    toAgent);

                Logger.Info($"OCSBot::Dialog:PostToAgent() - {JsonConvert.SerializeObject(toAgent)}");
                convStatus = (await agentStorage.QueryConversationStatusAsync(agent.Id)).OrderByDescending(o => o.Timestamp).FirstOrDefault();
                convStatus.OCSDirectlineConversationId = conversation.ConversationId;
                convStatus.OCSEndUserId = activityFromUser.From.Id;
                convStatus.OCSEndUserName = activityFromUser.From.Name;
                convStatus.OCSBotName = activityFromUser.Recipient.Name;
                convStatus.OCSBotId = activityFromUser.Recipient.Id;
                await agentStorage.UpdateConversationStatusAsync(convStatus);
                return null;
            }
            catch (Exception exp)
            {
                Logger.Info($"OCSBot::PostToAgent() - Exception while posting to Agent:{exp.Message}");
                throw;
            }

        }
    }
}