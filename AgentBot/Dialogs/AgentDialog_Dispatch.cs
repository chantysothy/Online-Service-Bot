using AgentBot.Configuration;
using AgentBot.Localizations;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.DirectLine;
using OCSBot.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OCSBot.Shared.Models;

namespace AgentBot.Dialogs
{
    
    public partial class AgentDialog 
    {
        public async Task DispatchAsync(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            Activity activity = (Activity)await item;
            Logger.Info($"message received from {activity.From.Name} : {JsonConvert.SerializeObject(activity)}");
            Logger.Info($"message received to {activity.Recipient.Name}/{activity.Recipient.Id}");

            var storage = new AgentStatusStorage(
                                    ConfigurationHelper.GetString("BotStatusDBConnectionString"));

            if (activity.From.Name.EndsWith("@ocsuser"))
            {
                //Messages from OCS User, when message from OCS User sent to this method, it has to be coming from DirectLine
                AgentStatus agent = null;
                //retrieve ChannelData which includes channelId for our conversation
                //TODO:figure out a way that simplier
                JObject o = (JObject)activity.ChannelData;
                var os = JsonConvert.SerializeObject(o);
                DirectLineChannelData channelData = JsonConvert.DeserializeObject<DirectLineChannelData>(os);

                Logger.Info($"ChannelData = {channelData}");
                //ConversationStatus conversation = null;
                Logger.Info($"RoundTrip = {channelData.RoundTrip}");
                //first message send to agent, find an available agent
                //TODO:the agent has been selected in OCS Bot, need to make it sync
                //      Instead of selecting another one here...
                agent = (await storage.FindAvailableAgentsAsync()).FirstOrDefault();
                var convRecord = (await storage.FindConversationActivityAsync(a => a.UserID == agent.Id)).FirstOrDefault();
                convRecord.RemoteConversationID = channelData.ConversationId;
                convRecord.RemoteBotId = activity.From.Id;//remote user id actually...
                convRecord.RemoteActivity = UrlToken.Encode<ResumptionCookie>(
                                                new ResumptionCookie((Activity)activity));
                await storage.UpdateConversationActivityAsync(convRecord);

                Logger.Info($"agent:{agent}");
                if (!agent.IsLoggedIn)
                {
                    //Agent somehow is occupied (logout?)
                    Logger.Info("Agent is occupied");
                    var reply = activity.CreateReply($"Agent is occupied");
                    ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
                else
                {
                    //Agnet is available to answer questions, send message to agent
                    Logger.Info("Sending to conversation...");

                    //TODO:Need to check if current agent is this user
                    //agent.IsOccupied = true;
                    //await storage.UpdateAgentStatusAsync(agent);

                    //First retrieve last conversaion if exists
                    //resumptionCookie = UrlToken.Decode<ResumptionCookie>(conversation.AgentResumptionCookie);
                    var localResumptionCookie = UrlToken.Decode<ResumptionCookie>(convRecord.LocalActivity);
                    Logger.Info($"AgentBot::LocalResumptionCookie:{localResumptionCookie}");
                    var localActivity = localResumptionCookie.GetMessage();
                    var localReply = localActivity.CreateReply($"[{activity.From.Name}]{activity.Text}");
                    var localConnector = new ConnectorClient(new Uri(localActivity.ServiceUrl),
                                                                new MicrosoftAppCredentials(
                                                                    ConfigurationHelper.GetString("MicrosoftAppId"),
                                                                    ConfigurationHelper.GetString("MicrosoftAppPassword")),
                                                                true);
                    Microsoft.Bot.Connector.Conversations localConversation = new Microsoft.Bot.Connector.Conversations(localConnector);
                    localConversation.ReplyToActivity(localReply);
                    Logger.Info("done");

                    
                    return;

                }
            }
            else
            {
                resumptionCookie = new ResumptionCookie(await item);
                await MessageReceivedAsync(context, item);
            }
        }
    }
}