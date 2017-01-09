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
            try
            {
                if (activity.Type == ActivityTypes.Message)
                {
                    var userData = activity.GetStateClient().BotState.GetUserData(activity.ChannelId, activity.From.Id);
                    bool authenticated = false;
                    try
                    {
                        authenticated = userData.GetProperty<bool>("Agent:Authenticated");
                    }
                    catch (Exception exp)
                    {

                    }
                    if (!authenticated)
                    {
                        var connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                        await connector.Conversations.ReplyToActivityAsync(activity.CreateReply($"Authenticated = {authenticated}"));
                        RequestLogin(activity);
                    }
                    else if (activity.Text.ToLower().Trim() == "logout")
                    {
                        userData.SetProperty<bool>("Agent:Authenticated", false);
                        var state = activity.GetStateClient().BotState;
                        state.SetUserData(activity.ChannelId, activity.From.Id, userData);
                        var connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                        await connector.Conversations.ReplyToActivityAsync(activity.CreateReply("you've logged out"));
                    }
                    else
                    {
                        //var reply = activity.CreateReply("you've logged in");
                        //ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                        //var resp = await connector.Conversations.SendToConversationAsync(reply);
                        await Conversation.SendAsync(activity, () => new Dialogs.AgentDialog());
                    }
                }
                else
                {
                    HandleSystemMessage(activity);
                }
            }catch(Exception exp)
            {
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