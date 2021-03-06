﻿using System;
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
                    if (activity.Text == "$demo")
                    {
                        Logger.Info($"OCSBot::message received : {JsonConvert.SerializeObject(activity)}");
                        await Conversation.SendAsync(activity, () => new Dialogs.DemoDialog());

                    }
                    else
                    {
                        Logger.Info($"OCSBot::message received : {JsonConvert.SerializeObject(activity)}");
                        await Conversation.SendAsync(activity, () => new Dialogs.AgentDialog());
                    }
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
                    ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
                    var reply = message.CreateReply($"{Messages.BOT_GET_HELP}");
                    connector.Conversations.ReplyToActivity(reply);
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