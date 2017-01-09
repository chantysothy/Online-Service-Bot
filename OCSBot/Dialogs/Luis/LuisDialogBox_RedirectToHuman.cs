using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.DirectLine;
using OCSBot.KB;
using OCSBot.Localizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace OCSBot.Dialogs
{   
    public partial class LuisDialogBox:LuisDialog<string>
    {
        public async Task HumanProcess(IDialogContext context, LuisResult result)
        {
            var resp = await PostToBotAsync(new Microsoft.Bot.Connector.DirectLine.Activity()
            {
                Type = Microsoft.Bot.Connector.DirectLine.ActivityTypes.Message,
                From = new Microsoft.Bot.Connector.DirectLine.ChannelAccount(context.Activity.Recipient.Id, context.Activity.Recipient.Name),
                Recipient = new Microsoft.Bot.Connector.DirectLine.ChannelAccount("default-user", "User"),
                ChannelId = "7kfikk9l7h182ke3i",
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
            
            //ConnectorClient connector = new ConnectorClient(new Uri(context.Activity.ServiceUrl));
            //StateClient sc = ((Microsoft.Bot.Connector.DirectLine.Activity)context.Activity).GetStateClient();

            //connector.Conversations.CreateDirectConversation(context.Activity.Recipient, new Microsoft.Bot.Connector.DirectLine.ChannelAccount());
            //BotData data = await sc.BotState.GetUserDataAsync(context.Activity.ChannelId, context.Activity.From.Id);
            //data.SetProperty("Authenticated", true);
            //data.SetProperty("IsAgent", true);

        }
        private string _agentConversationId = null;
        private string _agentConversationWatermark = null;
        private async Task<Microsoft.Bot.Connector.DirectLine.Activity> PostToBotAsync(Microsoft.Bot.Connector.DirectLine.Activity activityFromUser)
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
                    From = activityFromUser.Recipient,
                    Recipient = new Microsoft.Bot.Connector.DirectLine.ChannelAccount()
                };
                var resp = await dc.Conversations.PostActivityAsync(
                    _agentConversationId,
                    toAgent);

                var activitySet = await dc.Conversations.GetActivitiesAsync(_agentConversationId, _agentConversationWatermark);
                _agentConversationId = activitySet.Watermark;

                return activitySet.Activities.Last();
            }
            catch (Exception exp)
            {
                throw;
            }

        }
    }
}