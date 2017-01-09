using AgentBot.Configuration;
using AgentBot.Localizations;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AgentBot.Dialogs
{
    [Serializable]
    public class AgentDialog : IDialog<object>
    {
        private ResumptionCookie resumptionCookie = null;
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }
        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            var activity = await item;
            resumptionCookie = new ResumptionCookie(activity);
            bool authenticated = false;
            
            var userData = ((Activity)activity).GetStateClient().BotState.GetUserData(activity.ChannelId, activity.From.Id);
            try
            {
                authenticated = userData.GetProperty<bool>("Agent:Authenticated");
                await context.PostAsync($"authenticated={authenticated}");
            }
            catch
            {

            }
            if (!authenticated)
            {
                //RequestLogin((Activity)activity);
                await context.PostAsync("oops");
                context.Done("");
            }
            else
            {
                //Direct Line API ?
                await context.PostAsync("you've logged in");
                context.Done("");
            }

        }
        private void RequestLogin(Activity message)
        {
            BotData data = null;
            StateClient sc = message.GetStateClient();
            try
            {
                data = sc.BotState.GetUserData(message.ChannelId, message.From.Id);
                data.SetProperty<bool>("Authenticated", false);
            }
            catch (Exception exp)
            {

            }
            //Ask agnet to sign-in
            var reply = message.CreateReply(Messages.BOT_PLEASE_LOGIN);
            reply.Recipient = message.From;
            reply.Type = "message";
            reply.Attachments = new List<Attachment>();
            List<CardAction> cardButtons = new List<CardAction>();
            var resumptionCookie = new ResumptionCookie(message);
            var encodedCookie = UrlToken.Encode(resumptionCookie);
            //string encodedResumptionCookie = UrlToken.Encode(this.resumptionCookie);
            CardAction button = new CardAction()
            {
                Value = $"{ConfigurationHelper.GetString("AgentLogin_URL")}?cookie={encodedCookie}",
                Type = "signin",
                Title = "Connect"
            };
            cardButtons.Add(button);
            SigninCard plCard = new SigninCard(text: Messages.BOT_PLEASE_LOGIN, buttons: cardButtons);
            Attachment plAttachment = plCard.ToAttachment();
            reply.Attachments.Add(plAttachment);
            ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
            var response = connector.Conversations.SendToConversation(reply);
        }
    }
}