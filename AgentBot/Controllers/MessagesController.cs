using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using AgentBot.Localizations;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs;
using AgentBot.Configuration;
using OCSBot.Shared;
using OCSBot.Shared.Models;
using Newtonsoft.Json.Linq;

namespace AgentBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            Logger.Info($"message received : type is {activity.Type}");
            try
            {
                if (activity.Type == ActivityTypes.Message)
                {
                    Logger.Info($"message received from {activity.From.Name} : {JsonConvert.SerializeObject(activity)}");
                    Logger.Info($"message received to {activity.Recipient.Name}/{activity.Recipient.Id}");
                    await Conversation.SendAsync(activity, () => new Dialogs.AgentDialog());
                    //if (activity.From.Name.EndsWith("@ocsuser"))
                    //{
                    //    AgentStatus agent = null;
                    //    JObject o = (JObject)activity.ChannelData;
                    //    var os = JsonConvert.SerializeObject(o);
                    //    DirectLineChannelData channelData = JsonConvert.DeserializeObject<DirectLineChannelData>(os);
                    //    //dynamic channelData = activity.ChannelData;
                    //    Logger.Info($"ChannelData = {channelData}");
                    //    ConversationStatus conversation = null;
                    //    if (channelData.RoundTrip == 0)
                    //    {
                    //        Logger.Info($"RoundTrip = {channelData.RoundTrip}");
                    //        //first message send to agent, find an available agent
                    //        agent = (await storage.FindAvailableAgentsAsync()).FirstOrDefault();
                    //        conversation = (await storage.QueryConversationStatusAsync(c => c.AgentID == agent.Id)).FirstOrDefault();
                    //        Logger.Info($"conversation = {conversation}");

                    //        conversation.OCSDirectlineConversationId = channelData.ConversationId;
                    //    }
                    //    else
                    //    {
                    //        //continous messages, find previous agent
                    //        var previousConversation = (await storage.QueryConversationStatusAsync(c => c.OCSDirectlineConversationId == channelData.ConversationId)).SingleOrDefault();
                    //        Logger.Info($"previousConversation = {previousConversation}");

                    //        agent = (await storage.FindAvailableAgentsAsync(a => previousConversation.AgentID == a.AgentIdInChannel)).SingleOrDefault();
                    //        Logger.Info($"agent = {agent}");

                    //        conversation.OCSDirectlineConversationId = channelData.ConversationId;
                    //    }
                    //    Logger.Info($"agent:{agent}");
                    //    if (!agent.IsLoggedIn || agent.IsOccupied)
                    //    {
                    //        //Agent somehow is occupied (logout?)
                    //        Logger.Info("Agent is occupied");
                    //        var reply = activity.CreateReply($"Agent is occupied");
                    //        ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                    //        await connector.Conversations.ReplyToActivityAsync(reply);
                    //    }
                    //    else
                    //    {
                    //        //Agnet is available to answer questions, send message to agent
                    //        Logger.Info("Sending to conversation...");
                    //        if (channelData.RoundTrip == 0)
                    //        {
                    //            //first message sent to agent
                    //            conversation = (await storage.QueryConversationStatusAsync(agent.Id))
                    //                                        .Where(c => c.OCSDirectlineConversationId == channelData.ConversationId)
                    //                                        .SingleOrDefault();

                    //        }
                    //        //First retrieve last conversaion if exists
                    //        Logger.Info("AgentBot::Sending to agent...");

                    //        var connector = new ConnectorClient(new Uri(agent.ServiceURL),
                    //                                ConfigurationHelper.GetString("MicrosoftAppId"),
                    //                                ConfigurationHelper.GetString("MicrosoftAppPassword"));
                    //        var resp = connector
                    //                            .Conversations
                    //                            .SendToConversation(activity, agent.ConversationId);



                    //    }
                    //}
                    //else
                    //{
                    //    await Conversation.SendAsync(activity, () => new Dialogs.AgentDialog());
                    //}
                }
                else
                {
                    HandleSystemMessage(activity);
                }
            }
            catch (Exception exp)
            {
                Logger.Info($"Exception:{exp.Message}");
                Logger.Info($"Stack trace:{exp.StackTrace}");

                var reply = activity.CreateReply($"Error:{exp.Message} - {exp.StackTrace}");
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                await connector.Conversations.ReplyToActivityAsync(reply);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }
        private void RequestLogin(Activity message)
        {
            var resumptionCookie = new ResumptionCookie(message);
            var encodedResumptionCookie = UrlToken.Encode<ResumptionCookie>(resumptionCookie);
            Activity oriMessage = resumptionCookie.GetMessage();

            
            var reply = oriMessage.CreateReply(Messages.BOT_PLEASE_LOGIN);
            reply.Recipient = oriMessage.From;
            reply.Type = ActivityTypes.Message;
            reply.Attachments = new List<Attachment>();
            List<CardAction> cardButtons = new List<CardAction>();
            //var resumptionCookie = new ResumptionCookie(message);
            var encodedCookie = UrlToken.Encode(resumptionCookie);
            //string encodedResumptionCookie = UrlToken.Encode(this.resumptionCookie);
            CardAction button = new CardAction()
            {
                Value = $"{ConfigurationHelper.GetString("AgentLogin_URL")}?cookie={encodedCookie}",
                Type = "signin",
                Title = $"Authentication Required"
            };
            cardButtons.Add(button);
            SigninCard plCard = new SigninCard(
                text: $"{Messages.BOT_PLEASE_LOGIN} - {message.From.Id}/{message.From.Name}/{message.Conversation.Id}", 
                buttons: cardButtons);
            Attachment plAttachment = plCard.ToAttachment();
            reply.Attachments.Add(plAttachment);
            ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
            //Conversation.ResumeAsync(resumptionCookie, reply).Wait();
            var response = connector.Conversations.SendToConversation(reply);
        }
        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
                var user = message.MembersAdded.FirstOrDefault();
                if (user != null)
                {
                }
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}