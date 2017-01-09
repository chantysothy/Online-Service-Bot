using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using OCSBot.KB;
using OCSBot.Localizations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace OCSBot.Dialogs
{
    
    public partial class LuisDialogBox
    {
        
        [LuisIntent("QueryMailingStatus")]
        public async Task QueryMailingStatus(IDialogContext context, LuisResult result)
        {
            var options = new string[] { "國際及大陸各類郵件", "國內快捷/掛號/包裹","信箱掛件數量" };
            PromptOptions<string> prompt = new PromptOptions<string>
                    (prompt:"你要查詢的是哪種郵件？",
                    retry:"沒有這種郵件類型，請再輸入一次",
                    tooManyAttempts:"你已經輸入錯誤太多次了，等會再試試看吧!",
                    options: options);
            PromptDialog.Choice<string>(context, ResumeAfterQueryMailingStatus, prompt);
        }
        
        async Task ResumeAfterQueryMailingStatus(IDialogContext context, IAwaitable<string> result)
        {
            var input = await result;
            await context.PostAsync(
                    string.Format(Messages.BOT_PLEASE_INPUT_PACKAGE_NUMNER,input)
                );
            propertyBag.Add("QueryMailingStatus", input);
            switch(input)
            {
                case "國際及大陸各類郵件":
                    //直接輸入郵件號碼
                    context.Wait(ResumeAfterInformationInput);
                    break;
                case "國內快捷/掛號/包裹":
                    //直接輸入郵件號碼，可以逗號分開
                    context.Wait(ResumeAfterInformationInput);
                    break;
                case "信箱掛件數量":
                    //電腦局號（6碼）,信箱種類,信箱號碼,身分別,識別碼 (FormDialog)
                    break;
            }
        }

        async Task ResumeAfterInformationInput(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var input = await result;
            propertyBag.Add("PackageNumber", input.Text);
            //TODO:query database to get status of package
            await context.PostAsync(string.Format(Messages.BOT_REPLY_STATUS,
                                        propertyBag["QueryMailingStatus"],
                                        propertyBag["PackageNumber"],
                                        Messages.STATUS_DELIVERED));
            context.Done("");
        }
    }
}