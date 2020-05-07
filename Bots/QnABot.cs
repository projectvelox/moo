// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Linq;
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
using Microsoft.Extensions.Configuration;

using EchoBot.OmniChannel;
using Microsoft.Extensions.Logging;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class QnABot<T> : ActivityHandler where T : Microsoft.Bot.Builder.Dialogs.Dialog
    {
        protected readonly BotState ConversationState;
        protected readonly Microsoft.Bot.Builder.Dialogs.Dialog Dialog;
        protected readonly BotState UserState;

        public QnABot(ConversationState conversationState, UserState userState, T dialog, IConfiguration configuration)
        {
            ConversationState = conversationState;
            UserState = userState;
            Dialog = dialog;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        /* public QnaBot(QnAMakerEndpoint endpoint)
        {
            // connects to QnA Maker endpoint for each turn
            EchoBotQnA = new QnAMaker(endpoint);
        } */


        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occured during the turn.
            await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await UserState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            //string activity = turnContext.Activity.Text;

            await turnContext.SendActivityAsync(MessageFactory.Text($"Echo: {turnContext.Activity.Text}"), cancellationToken);
            await AccessQnAMaker(turnContext, cancellationToken);
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

        private async Task AccessQnAMaker(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {

            try
            {
                var httpClient = new HttpClient();

                var qnaMaker = new QnAMaker(new QnAMakerEndpoint
                {
                    KnowledgeBaseId = Configuration.GetValue<string>($"QnAKnowledgebaseId"),
                    EndpointKey = Configuration.GetValue<string>($"QnAAuthKey"),
                    Host = Configuration.GetValue<string>($"QnAEndpointHostName")
                },
                null,
                httpClient);

                var options = new QnAMakerOptions { Top = 1 };
                var results = await qnaMaker.GetAnswersAsync(turnContext, options);

                var test = Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
                await turnContext.SendActivityAsync(MessageFactory.Text(test.Result.Text), cancellationToken);

                /* if (results.Any())
                {
                    string test = await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
                    //await turnContext.SendActivityAsync(MessageFactory.Text("QnA Maker Returned: " + results.First().Answer), cancellationToken);
                    await turnContext.SendActivityAsync(MessageFactory.Text(test), cancellationToken);
                }
                else
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("Sorry, could not find an answer in the Q and A system."), cancellationToken);
                } */
            }

            catch (Exception ex) {
                await turnContext.SendActivityAsync(MessageFactory.Text(ex.ToString()), cancellationToken);
            }
        }
    }
}
