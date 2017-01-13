using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using OCSBot.Shared;
using OCSBot.Configuration;

namespace OCSBot.Dialogs
{
    [Serializable]
    public class AgentReplyDialog : IDialog<string>
    {
        public async Task StartAsync(IDialogContext context)
        {
            Logger.Info("StartAsync...");
            var incoming = context.Activity as Activity;
            var jsonText = JsonConvert.SerializeObject(incoming.ChannelData);
            Logger.Info($"JsonText={jsonText}");
            var channelData = JsonConvert.DeserializeObject<OCSBot.Shared.Models.DirectLineChannelData>(jsonText);
            Logger.Info($"ChannelData.DirectLineBotID={channelData.DirectLineBotID}");
            var storage = new AgentStatusStorage(ConfigurationHelper.GetString("BotStatusDBConnectionString"));
            var localConversation = (await storage.FindMyConversationActivityAsync(channelData.UserID)).FirstOrDefault();
            Logger.Info($"localConversation={localConversation}");

            var resumptionCookie = UrlToken.Decode<ResumptionCookie>(localConversation.LocalActivity);
            var localActivity = resumptionCookie.GetMessage();
            var reply = localActivity.CreateReply(((Activity)context.Activity).Text);
            Logger.Info($"reply={JsonConvert.SerializeObject(reply)}");
            var localConnector = new ConnectorClient(new Uri(localActivity.ServiceUrl),
                                                        new MicrosoftAppCredentials(
                                                            ConfigurationHelper.GetString("MicrosoftAppId"),
                                                            ConfigurationHelper.GetString("MicrosoftAppPassword")),
                                                        true);
            Logger.Info("A");
            Microsoft.Bot.Connector.Conversations localConversations = new Microsoft.Bot.Connector.Conversations(localConnector);
            localConversations.ReplyToActivity(reply);
            Logger.Info("Done");
        }
    }
}