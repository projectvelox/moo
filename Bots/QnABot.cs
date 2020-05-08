// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Schema;
using EchoBot.OmniChannel;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.BotBuilderSamples
{
    public class QnABot : ActivityHandler
    {
        protected readonly BotState ConversationState;
        protected readonly Microsoft.Bot.Builder.Dialogs.Dialog Dialog;
        protected readonly BotState UserState;


        public QnABot(ConversationState conversationState, UserState userState, T dialog)
        {
            ConversationState = conversationState;
            UserState = userState;
            Dialog = dialog;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public QnABot(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            try
            {
<<<<<<< HEAD

                OmnichannelBotClient.BridgeBotMessage(turnContext.Activity);
                await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
                await turnContext.SendActivityAsync(MessageFactory.Text($"Hello and welcome!"), cancellationToken);


                var httpClient = _httpClientFactory.CreateClient();

=======
                var httpClient = _httpClientFactory.CreateClient();
>>>>>>> parent of d51d80a... DUDE
                var qnaMaker = new QnAMaker(new QnAMakerEndpoint
                {
                    KnowledgeBaseId = _configuration["QnAKnowledgebaseId"],
                    EndpointKey = _configuration["QnAEndpointKey"],
                    Host = _configuration["QnAEndpointHostName"]
                },
                null,
                httpClient);

                _logger.LogInformation("Calling QnA Maker");

                var options = new QnAMakerOptions { Top = 1 };

                // The actual call to the QnA Maker service.
                var response = await qnaMaker.GetAnswersAsync(turnContext, options);
<<<<<<< HEAD
                if (response != null && response.Length > 0)
=======
                await turnContext.SendActivityAsync(MessageFactory.Text(response[0].Answer), cancellationToken);

                /*if (response != null && response.Length > 0)
>>>>>>> parent of d51d80a... DUDE
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(response[0].Answer), cancellationToken);
                }
                else
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("No QnA Maker answers were found."), cancellationToken);
                }
            }
            catch (Exception ex)
            {
                await turnContext.SendActivityAsync(MessageFactory.Text(ex.ToString()), cancellationToken);
            }
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    OmnichannelBotClient.BridgeBotMessage(turnContext.Activity);
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Hello and welcome!"), cancellationToken);
                }
            }
        }

    }
}
