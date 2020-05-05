// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using EchoBot.OmniChannel;
using Microsoft.Extensions.Logging;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class QnABot<T> : ActivityHandler where T : Microsoft.Bot.Builder.Dialogs.Dialog
    {
        protected readonly BotState ConversationState;
        protected readonly Microsoft.Bot.Builder.Dialogs.Dialog Dialog;
        protected readonly BotState UserState;

        public QnABot(ConversationState conversationState, UserState userState, T dialog)
        {
            ConversationState = conversationState;
            UserState = userState;
            Dialog = dialog;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occured during the turn.
            await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await UserState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

       protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
         {
            // Run the Dialog with the new message Activity.
            //await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
            var activity = await result as Activity;
            var text = (activity.Text ?? string.Empty);
            var url = "https://mooqnakb.azurewebsites.net/qnamaker/knowledgebases/bbb9cb8b-bef5-44b3-b3f0-c4fe30a4e63d/generateAnswer";
            var httpContent = new StringContent("{'question':'" + text + "'}", Encoding.UTF8, "application/json");

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", "EndpointKey 68bddf3c-07d6-47cd-91a9-d49fc575ee7b");
            var httpResponse = await httpClient.PostAsync(url, httpContent);
            var httpResponseMessage = await httpResponse.Content.ReadAsStringAsync();
            dynamic httpResponseJson = JsonConvert.DeserializeObject(httpResponseMessage);
            var replyMessage = httpResponseJson.answers[0].answer;
            
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {

                // Replace with your own message
                IActivity replyActivity = MessageFactory.Text($"{turnContext.Activity.Text}");

                // Replace with your own condition for bot escalation
                if (turnContext.Activity.Text.Equals("escalate", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Dictionary<string, object> contextVars = new Dictionary<string, object>() { { "BotHandoffTopic", "CreditCard" } };
            OmnichannelBotClient.AddEscalationContext(replyActivity, contextVars);
                    }
                    // Replace with your own condition for bot end conversation
                    else if (turnContext.Activity.Text.Equals("endconversation", StringComparison.InvariantCultureIgnoreCase))
                    {
                        OmnichannelBotClient.AddEndConversationContext(replyActivity);
                    }
                    // Call method BridgeBotMessage for every response that needs to be delivered to the customer.
                    else
                    {
                        OmnichannelBotClient.BridgeBotMessage(replyActivity);
                    }

                //await turnContext.SendActivityAsync(replyActivity, cancellationToken);
                //await turnContext.SendActivityAsync(turnContext, cancellationToken);


                //await turnContext.SendActivityAsync(Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken), cancellationToken);

                turnContext.SendActivityAsync(replyActivity, cancellationToken);
                //await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);

            }


        }
        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Hello and welcome!"), cancellationToken);
                }
            }
        }
    }
}
