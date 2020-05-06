// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.AI.QnA.Dialogs;
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
        private readonly IHttpClientFactory _httpClientFactory;

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
            var httpClient = _httpClientFactory.CreateClient();

            var qnaMaker = new QnAMaker(new QnAMakerEndpoint
            {
                KnowledgeBaseId = "bbb9cb8b-bef5-44b3-b3f0-c4fe30a4e63d",
                EndpointKey = "EndpointKey 68bddf3c-07d6-47cd-91a9-d49fc575ee7b",
                Host = "mooqnakb.azurewebsites.net"
            },
            null,
            httpClient);

            var options = new QnAMakerOptions { Top = 1 };

            // The actual call to the QnA Maker service.
            var response = await qnaMaker.GetAnswersAsync(turnContext, options);
            if (response != null && response.Length > 0)
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(response[0].Answer), cancellationToken);
            }
            else
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("No QnA Maker answers were found."), cancellationToken);
            }
        

        /*if (turnContext.Activity.Type == ActivityTypes.Message)
        {

            // Replace with your own message
            IActivity replyActivity = MessageFactory.Text($"{response[0].Answer}");

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

            turnContext.SendActivityAsync(replyActivity, cancellationToken);
            //await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);

        }
        */

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
