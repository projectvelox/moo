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

namespace Microsoft.BotBuilderSamples.Bots
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public class QnABot<T> : ActivityHandler where T : Microsoft.Bot.Builder.Dialogs.Dialog
    {
        protected readonly BotState ConversationState;
        protected readonly Microsoft.Bot.Builder.Dialogs.Dialog Dialog;
        protected readonly BotState UserState;

        private const string endpointVar = _configuration["QnAEndpointHostName"];
        private const string endpointKeyVar = _configuration["QnAEndpointKey"];
        private const string kbIdVar = _configuration["QnAKnowledgebaseId"];

        private static readonly string endpoint = Environment.GetEnvironmentVariable(endpointVar);
        private static readonly string endpointKey = Environment.GetEnvironmentVariable(endpointKeyVar);
        private static readonly string kbId = Environment.GetEnvironmentVariable(kbIdVar);

        public QnABot(ConversationState conversationState, UserState userState, T dialog)
        {
            ConversationState = conversationState;
            UserState = userState;
            Dialog = dialog;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occured during the turn.
            await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await UserState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            try
            {
                var uri = endpointVar + "/qnamaker/knowledgebases/" + kbIdVar + "/generateAnswer";

                // JSON format for passing question to service
                string question = @"{'question': '" + turnContext.Activity.Text + "?','top': 3}";

                await turnContext.SendActivityAsync(MessageFactory.Text(uri), cancellationToken);

                // Create http client
                //using (var client = new HttpClient())
                /*using (var request = new HttpRequestMessage())
                {
                    // POST method
                    request.Method = HttpMethod.Post;

                    // Add host + service to get full URI
                    request.RequestUri = new Uri(uri);

                    // set question
                    request.Content = new StringContent(question, Encoding.UTF8, "application/json");

                    // set authorization
                    request.Headers.Add("Authorization", "EndpointKey " + endpointKey);

                    // Send request to Azure service, get response
                    var response = client.SendAsync(request).Result;
                    var jsonResponse = response.Content.ReadAsStringAsync().Result;

                    // Output JSON response
                    await turnContext.SendActivityAsync(MessageFactory.Text(jsonResponse), cancellationToken);
                }*/
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
