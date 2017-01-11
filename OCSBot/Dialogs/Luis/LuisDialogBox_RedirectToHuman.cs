using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.DirectLine;
using OCSBot.KB;
using OCSBot.Localizations;
using OCSBot.Shared;
using OCSBot.Shared.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace OCSBot.Dialogs
{   
    public partial class LuisDialogBox:LuisDialog<string>
    {
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

                var connector = new ConnectorClient(new Uri(context.Activity.ServiceUrl));
                connector.Conversations.ReplyToActivity(context.Activity.Conversation.Id,
                                                            context.Activity.Id,
                                                            new Microsoft.Bot.Connector.Activity
                                                            {
                                                                Type = Microsoft.Bot.Connector.DirectLine.ActivityTypes.Message,
                                                                Text = resp.Text,
                                                                From = context.Activity.Recipient,
                                                                Recipient = context.Activity.From,
                                                                ChannelId = context.Activity.ChannelId
                                                            });

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
                return false;
            }

        }
        private string _agentConversationId = null;
        private string _agentConversationWatermark = null;
        private async Task<Microsoft.Bot.Connector.DirectLine.Activity> PostToAgentBotAsync(Microsoft.Bot.Connector.DirectLine.Activity activityFromUser)
        {
            var directLineSecret = Configuration.ConfigurationHelper.GetString("AgentBot_DirectLine_Secret");
            var dc = new DirectLineClient(directLineSecret, null);
            if (string.IsNullOrEmpty(_agentConversationId))
            {
                var conv = dc.Conversations.StartConversation();
                _agentConversationId = conv.ConversationId;

                var acts = dc.Conversations.GetActivities(_agentConversationId, _agentConversationWatermark);
            }
            try
            {
                var toAgent = new Microsoft.Bot.Connector.DirectLine.Activity
                {
                    Text = activityFromUser.Text,
                    From = activityFromUser.From,
                    Recipient = activityFromUser.Recipient
                };
                //var conversation = dc.Conversations.ReconnectToConversation(_agentConversationId);
                var conversation = dc.Conversations.StartConversation();
                var resp = await dc.Conversations.PostActivityAsync(
                                conversation.ConversationId,
                                toAgent);

                var activitySet = await dc.Conversations.GetActivitiesAsync(_agentConversationId, _agentConversationWatermark);
                //_agentConversationId = activitySet.Watermark;

                return activitySet.Activities.Last();
            }
            catch (Exception exp)
            {
                throw;
            }

        }
    }
}