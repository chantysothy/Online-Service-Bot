using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.DirectLine;
using Newtonsoft.Json;
using OCSBot.KB;
using OCSBot.Localizations;
using OCSBot.Shared;
using OCSBot.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace OCSBot.Dialogs
{   
    public partial class LuisDialogBox:LuisDialog<string>
    {
        private void SetHumanInvestigating(bool isHumanInvestigating, string endUserId, string endUserName)
        {
            IsHumanInvestigating = isHumanInvestigating;
            if (IsHumanInvestigating)
                EndUser = new Dictionary<string, string> { { "Id", endUserId }, { "Name", endUserName } };//context.Activity.From;
            else
                EndUser = null;
        }
        public Dictionary<string,string> EndUser { get; set; }
        public bool IsHumanInvestigating { get; set; } = false;
        public async Task<bool> HumanProcess(IDialogContext context, LuisResult result)
        {
            var statusDB = Configuration.ConfigurationHelper.GetString("BotStatusDBConnectionString");
            var db = new AgentStatusStorage(statusDB);
            var agents = await db.FindAvailableAgentsAsync();
            var agent = agents.FirstOrDefault();
            if (agent != null)
            {
                //from bot to agent
                var resp = await PostToAgentBotAsync(new Microsoft.Bot.Connector.DirectLine.Activity()
                {
                    Type = Microsoft.Bot.Connector.DirectLine.ActivityTypes.Message,
                    //From = new Microsoft.Bot.Connector.DirectLine.ChannelAccount(context.Activity.Recipient.Id, context.Activity.Recipient.Name),
                    From = new Microsoft.Bot.Connector.DirectLine.ChannelAccount(id: agent.BotIdInChannel),
                    Recipient = new Microsoft.Bot.Connector.DirectLine.ChannelAccount(id: agent.AgentIdInChannel),
                    ChannelId = agent.ChannelId,
                    Text = context.Activity.AsMessageActivity().Text
                });

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
        private string _agentConversationId = null;
        private string _agentConversationWatermark = null;
        private async Task<Microsoft.Bot.Connector.DirectLine.Activity> PostToAgentBotAsync2(Microsoft.Bot.Connector.DirectLine.Activity activityFromUser)
        {
            var directLineSecret = Configuration.ConfigurationHelper.GetString("AgentBot_DirectLine_Secret");
            var agentStatusDB = Configuration.ConfigurationHelper.GetString("BotStatusDBConnectionString");
            var agentStorage = new AgentStatusStorage(agentStatusDB);
            var agent = await agentStorage.QueryAgentStatusAsync(activityFromUser.Recipient.Id);

            using (var client = new ConnectorClient(new Uri("https://smba.trafficmanager.net/apis")))
            {
                var recipient = new Microsoft.Bot.Connector.ChannelAccount(/*agent.AgentIdInChannel*/"29:1Gk7vrlkdaMoN6fCtrycxkJfHcPS8zvi49Gukq4XuZAo");
                var from = new Microsoft.Bot.Connector.ChannelAccount("");
                //var conversatoin = await client.Conversations.CreateDirectConversationAsync(from, recipient);
                var message = new Microsoft.Bot.Connector.Activity
                {
                    Text = activityFromUser.Text,
                    From = from,
                    Conversation = new Microsoft.Bot.Connector.ConversationAccount
                    {
                        Id = agent.ConversationId
                    },
                    Recipient = recipient
                };
                var response = await client.Conversations.SendToConversationAsync(message);

                return null;
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

                //var test = convs.GetActivities(convStatus.ConversationId);
                
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
                var toAgent = new Microsoft.Bot.Connector.DirectLine.Activity
                {
                    Type = Microsoft.Bot.Connector.DirectLine.ActivityTypes.Message,
                    Text = activityFromUser.Text,
                    From = new Microsoft.Bot.Connector.DirectLine.ChannelAccount
                    {
                        Id = agent.Id,/*activityFromUser.From.Id,*/
                        Name = $"{activityFromUser.From.Name}@ocsuser"
                    },
                    Recipient = activityFromUser.Recipient,
                    ChannelId = agent.ChannelId,
                    ChannelData = new DirectLineChannelData
                    {
                        RoundTrip = 0,
                        ConversationId = _agentConversationId
                    }
                };

                var resp = await dc.Conversations.PostActivityAsync(
                                                    conversation.ConversationId,
                                                    toAgent);
                Logger.Info($"OCSBot::PostToAgent() - {JsonConvert.SerializeObject(toAgent)}");
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