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

namespace AgentBot.Dialogs
{
    [Serializable]
    public partial class AgentDialog : IDialog<object>
    {
        private ResumptionCookie resumptionCookie = null;

        private void PostToOCSUser(string agentId,string text)
        {
            //DirectLineClient dc = new DirectLineClient()
        }
        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            Logger.Info("message received async");
            var activity = await item;
            bool authenticated = false;

            var userData = ((Activity)activity).GetStateClient()
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
            if (activity.From.Name.EndsWith("@ocsuser"))
            {
                //message from user, send to agent
                Logger.Info($"message sent into AgentDialog...");
                await DispatchAsync(context, item);
            }
            else
            {
                //message from agent
                if (activity.Text.ToLower().Trim() == "login")
                {
                    RequestLogin((Activity)activity);
                    resumptionCookie = new ResumptionCookie(activity);

                    Logger.Info($"ResumptionCookie set!");
                }
                else if (activity.Text.ToLower().Trim() == "logout")
                {
                    authenticated = false;
                    userData.SetProperty<bool>("Agent:Authenticated", false);

                    ((Activity)activity).GetStateClient()
                        .BotState
                        .SetUserData(activity.ChannelId, activity.From.Id, userData);

                    var connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                    await connector.Conversations.ReplyToActivityAsync(((Activity)activity).CreateReply("you've logged out"));
                }
                else if (!authenticated)
                {
                    RequestLogin((Activity)activity);
                    context.Wait(DispatchAsync);
                    //context.Done("");
                }
                else
                {
                    //send to OCS user
                    await context.PostAsync($"[logged in]{activity.Text}");
                    PostToOCSUser(activity.From.Id, activity.Text);
                    context.Wait(DispatchAsync);
                    //context.Done("");
                }
            }
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
            var encodedCookie = UrlToken.Encode(resumptionCookie);
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

        public async Task StartAsync(IDialogContext context)
        {
            Logger.Info("StartAsync");
            //context.Wait(MessageReceivedAsync);
            context.Wait(DispatchAsync);
        }
    }
}