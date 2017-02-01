using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.Bot.Connector;

namespace AgentBot.Dialogs
{
    [Serializable]
    public class DemoDialog : IDialog<object>
    {
        ResumptionCookie cookie = null;
        SubscriptionClient Client = null;
        public async Task StartAsync(IDialogContext context)
        {
            cookie = new ResumptionCookie(((Activity)context.Activity));
            //var connString = "Endpoint=sb://chatbottest.servicebus.windows.net/;SharedAccessKeyName=admin;SharedAccessKey=ZO3HDEWCmvS5XhhSB1ZafL6WJfBYvdiMoCqLacA9fyQ=";
            var connString = "Endpoint=sb://chatbottest.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=IQe+us9NR022Jf1y9qyFm1sbTnu80xHhrcFo3cSgTZw=";
            var namespaceManager =
                NamespaceManager.CreateFromConnectionString(connString);
            
            if (!namespaceManager.SubscriptionExists("conversation","test"))
            {
                SqlFilter idFilder =
                                new SqlFilter("id='test'");
                var sd = namespaceManager.CreateSubscription("conversation", "test");
                       //"test",
                       //idFilder);
            }

            if (Client == null)
            {
                Client =
                    SubscriptionClient.CreateFromConnectionString
                        (connString, "conversation", "test");
            
            OnMessageOptions options = new OnMessageOptions();
            options.AutoComplete = false;
            options.AutoRenewTimeout = TimeSpan.FromMinutes(1);
            
            Client.OnMessage(
                    async (message) =>
                    {
                        Activity incoming = cookie.GetMessage();
                        try
                        {
                            var text = message.EnqueuedTimeUtc.ToString();
                            Activity reply = incoming.CreateReply(text);
                            await context.PostAsync(reply);

                            //if (!message.IsBodyConsumed)
                            //{
                            //    await message.CompleteAsync();
                            //}
                        }
                        catch (Exception exp)
                        {
                            //await message.AbandonAsync();
                            //Activity reply = incoming.CreateReply(exp.Message);
                            //await context.PostAsync(reply);

                            //if (!message.IsBodyConsumed)
                            //{
                            //    await message.CompleteAsync();
                            //}
                        }
                        
                    },
                    options
                );
            }
            //Client.OnMessageAsync(
            //        async (message) =>
            //        {
            //            Activity incoming = cookie.GetMessage();
            //            var replyText = $"test:{DateTime.Now}";// $"message:{message.ToString()}";
            //            Activity reply = incoming.CreateReply(replyText);
            //            await context.PostAsync(reply);

            //            message.Complete();
            //        },
            //        options
            //    );
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<Microsoft.Bot.Connector.IMessageActivity> item)
        {
            cookie = new ResumptionCookie(((Activity)context.Activity));

            Activity incoming = (Activity)(await item);
            if (incoming.Text == "$end")
            {
                context.Done("");
            }
            else
            {
                Activity reply = incoming.CreateReply($"echo {incoming.Text}");
                await context.PostAsync(reply);
            }
        }
    }
}