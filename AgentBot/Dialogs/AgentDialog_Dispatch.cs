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
            Logger.Info("dispatch()");
            Activity activity = (Activity)await item;
            Logger.Info($"message received from {activity.From.Name} : {JsonConvert.SerializeObject(activity)}");
            Logger.Info($"message received to {activity.Recipient.Name}/{activity.Recipient.Id}");

            var storage = new AgentStatusStorage(
                                    ConfigurationHelper.GetString("BotStatusDBConnectionString"));

            if (activity.From.Name.EndsWith("@ocsuser"))
            {
                AgentStatus agent = null;
                JObject o = (JObject)activity.ChannelData;
                var os = JsonConvert.SerializeObject(o);
                DirectLineChannelData channelData = JsonConvert.DeserializeObject<DirectLineChannelData>(os);
                //dynamic channelData = activity.ChannelData;
                Logger.Info($"ChannelData = {channelData}");
                ConversationStatus conversation = null;
                if (channelData.RoundTrip == 0)
                {
                    Logger.Info($"RoundTrip = {channelData.RoundTrip}");
                    //first message send to agent, find an available agent
                    agent = (await storage.FindAvailableAgentsAsync()).FirstOrDefault();
                    conversation = (await storage.QueryConversationStatusAsync(c => c.AgentID == agent.Id)).FirstOrDefault();
                    Logger.Info($"conversation = {conversation}");

                    conversation.OCSDirectlineConversationId = channelData.ConversationId;
                }
                else
                {
                    //continous messages, find previous agent
                    var previousConversation = (await storage.QueryConversationStatusAsync(c => c.OCSDirectlineConversationId == channelData.ConversationId)).SingleOrDefault();
                    Logger.Info($"previousConversation = {previousConversation}");

                    agent = (await storage.FindAvailableAgentsAsync(a => previousConversation.AgentID == a.AgentIdInChannel)).SingleOrDefault();
                    Logger.Info($"agent = {agent}");

                    conversation.OCSDirectlineConversationId = channelData.ConversationId;
                }
                Logger.Info($"agent:{agent}");
                if (!agent.IsLoggedIn || agent.IsOccupied)
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
                    if (channelData.RoundTrip == 0)
                    {
                        //first message sent to agent
                        conversation = (await storage.QueryConversationStatusAsync(agent.Id))
                                                    .Where(c => c.OCSDirectlineConversationId == channelData.ConversationId)
                                                    .SingleOrDefault();

                    }
                    //First retrieve last conversaion if exists
                    resumptionCookie = UrlToken.Decode<ResumptionCookie>(conversation.AgentResumptionCookie);
                    Logger.Info($"AgentBot::Sending to agent...resumptionCookie={resumptionCookie}");
                    var originalActivity = resumptionCookie.GetMessage();
                    var reply = originalActivity.CreateReply(activity.Text);


                    Logger.Info($"AgentBot::Sending:{JsonConvert.SerializeObject(reply)}");
                    //await context.PostAsync(reply);
                    //Logger.Info($"AgentBot::Sending to agent...done");
                    var connector = new ConnectorClient(new Uri(agent.ServiceURL),
                                new MicrosoftAppCredentials(
                                    ConfigurationHelper.GetString("MicrosoftAppId"),
                                    ConfigurationHelper.GetString("MicrosoftAppPassword")),
                                true);
                    
                    Microsoft.Bot.Connector.Conversations convs = new Microsoft.Bot.Connector.Conversations(connector);
                    convs.ReplyToActivity(reply);


                    var resp = connector
                                    .Conversations
                                    .SendToConversation(reply, originalActivity.Conversation.Id);
                }
            }
            else
            {
                Logger.Info($"ResumptionCookie set!");
                resumptionCookie = new ResumptionCookie(await item);
                await MessageReceivedAsync(context, item);
            }
        }
    }
}