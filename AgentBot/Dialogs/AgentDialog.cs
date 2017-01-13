using AgentBot.Configuration;
using AgentBot.Localizations;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.DirectLine;
using Newtonsoft.Json;
using OCSBot.Shared;
using OCSBot.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AgentBot.Dialogs
{
    [Serializable]
    public partial class AgentDialog : IDialog<object>
    {
        private ResumptionCookie resumptionCookie = null;

        private async Task PostToOCSUser(IDialogContext context, Microsoft.Bot.Connector.Activity activity)
        {
            Logger.Info($"Agent [{activity.From.Id}] is replying");
            var storage = new AgentStatusStorage(ConfigurationHelper.GetString("BotStatusDBConnectionString"));
            AgentStatus agent = await storage.QueryAgentStatusAsync(activity.From.Id);
            //DirectLineClient dc = new DirectLineClient()
            var uri = new Uri("https://directline.botframework.com");
            Logger.Info($"PostToOCSUser::{agent.Id}/{agent.Name}");
            DirectLineClientCredentials creds = new DirectLineClientCredentials(ConfigurationHelper.GetString("OCSBot_DirectLine_Secret")); //lot into the bot framework
            Microsoft.Bot.Connector.DirectLine.DirectLineClient client = new Microsoft.Bot.Connector.DirectLine.DirectLineClient(uri, creds); //connect the client
            var conversation = client.Conversations.StartConversation();
            client.Conversations.PostActivity(conversation.ConversationId,
                                                new Microsoft.Bot.Connector.DirectLine.Activity
                                                {
                                                    From = new Microsoft.Bot.Connector.DirectLine.ChannelAccount
                                                    {
                                                        Id = agent.Id,
                                                        Name = $"{agent.Name}@agent"
                                                    },
                                                    Type = Microsoft.Bot.Connector.ActivityTypes.Message,
                                                    Text = activity.Text
                                                });

            //var remoteConnector = new ConnectorClient(
            //                            baseUri: new Uri(remoteActivity.ServiceUrl),
            //                            credentials: new MicrosoftAppCredentials(
            //                                            appId: ConfigurationHelper.GetString("MicrosoftAppId"),
            //                                            password: ConfigurationHelper.GetString("MicrosoftAppPassword")
            //                                        ),
            //                            addJwtTokenRefresher: true
            //                            );
            //Logger.Info($"remoteActivity={JsonConvert.SerializeObject(remoteActivity)}");
            //remoteConnector.Conversations.SendToConversation(reply);

            ////reply.From.Name += activity.From.Name + "@agent";
            //Logger.Info($"reply created:{JsonConvert.SerializeObject(reply)}");
            //remoteConnector.Conversations.ReplyToActivity(reply);
            //Logger.Info($"replied");
        }
        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<Microsoft.Bot.Connector.IMessageActivity> item)
        {
            Logger.Info("MessageReceivedAsync() called");
            var activity = await item;
            bool authenticated = false;

            var userData = ((Microsoft.Bot.Connector.Activity)activity).GetStateClient()
                            .BotState
                            .GetUserData(activity.ChannelId, activity.From.Id);
            try
            {
                authenticated = userData.GetProperty<bool>("Agent:Authenticated");
                await context.PostAsync($"authenticated={authenticated}");
            }
            catch
            {

            }
            //Do we really need this check ?
            if (activity.From.Name.EndsWith("@ocsuser"))
            {
                //message from user, send to agent
                Logger.Info($"message sent into AgentDialog...");
                await DispatchAsync(context, item);
            }
            else
            {
                var agentText = activity.Text.ToLower().Trim();
                //message from agent
                if (agentText == "login")
                {
                    RequestLogin((Microsoft.Bot.Connector.Activity)activity);
                    resumptionCookie = new ResumptionCookie(activity);

                    Logger.Info($"ResumptionCookie set!");
                }
                else if (agentText == "logout")
                {
                    authenticated = false;
                    userData.SetProperty<bool>("Agent:Authenticated", false);

                    ((Microsoft.Bot.Connector.Activity)activity).GetStateClient()
                        .BotState
                        .SetUserData(activity.ChannelId, activity.From.Id, userData);

                    var connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                    await connector.Conversations.ReplyToActivityAsync(((Microsoft.Bot.Connector.Activity)activity).CreateReply("you've logged out"));
                }
                else if (agentText.StartsWith("reply:"))
                {
                    //this is messages replies to user
                    await PostToOCSUser(context,(Microsoft.Bot.Connector.Activity)activity);
                    
                }
                else if (!authenticated)
                {
                    RequestLogin((Microsoft.Bot.Connector.Activity)activity);
                    context.Wait(DispatchAsync);
                    //context.Done("");
                }
                else
                {
                    //send to OCS user
                    await context.PostAsync($"[logged in]{activity.Text}");
                    await PostToOCSUser(context, (Microsoft.Bot.Connector.Activity)activity);
                    context.Wait(DispatchAsync);
                    //context.Done("");
                }
            }
        }

        private void RequestLogin(Microsoft.Bot.Connector.Activity message)
        {
            var resumptionCookie = new ResumptionCookie(message);
            var encodedResumptionCookie = UrlToken.Encode<ResumptionCookie>(resumptionCookie);
            Microsoft.Bot.Connector.Activity oriMessage = resumptionCookie.GetMessage();


            var reply = oriMessage.CreateReply(Messages.BOT_PLEASE_LOGIN);
            reply.Recipient = oriMessage.From;
            reply.Type = Microsoft.Bot.Connector.ActivityTypes.Message;
            reply.Attachments = new List<Microsoft.Bot.Connector.Attachment>();
            List<Microsoft.Bot.Connector.CardAction> cardButtons = new List<Microsoft.Bot.Connector.CardAction>();
            var encodedCookie = UrlToken.Encode(resumptionCookie);
            Microsoft.Bot.Connector.CardAction button = new Microsoft.Bot.Connector.CardAction()
            {
                Value = $"{ConfigurationHelper.GetString("AgentLogin_URL")}?cookie={encodedCookie}",
                Type = "signin",
                Title = $"Authentication Required"
            };
            cardButtons.Add(button);
            Microsoft.Bot.Connector.SigninCard plCard = new Microsoft.Bot.Connector.SigninCard(
                text: $"{Messages.BOT_PLEASE_LOGIN} - {message.From.Id}/{message.From.Name}/{message.Conversation.Id}",
                buttons: cardButtons);
            Microsoft.Bot.Connector.Attachment plAttachment = plCard.ToAttachment();
            reply.Attachments.Add(plAttachment);
            ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
            //Conversation.ResumeAsync(resumptionCookie, reply).Wait();
            var response = connector.Conversations.SendToConversation(reply);
        }

        public async Task StartAsync(IDialogContext context)
        {
            Logger.Info("StartAsync");
            //context.Wait(MessageReceivedAsync);
            context.Wait(DispatchAsync);
        }
    }
}