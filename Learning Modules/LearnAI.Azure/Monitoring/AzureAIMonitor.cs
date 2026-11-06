using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace LearnAI.Azure.Monitoring
{
    public class AzureAIMonitor : DelegatingChatClient
    {
        private readonly ILogger<AzureAIMonitor> _logger;

        public AzureAIMonitor(IChatClient innerClient, ILogger<AzureAIMonitor> logger)
            : base(innerClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Initiating model inference request execution pipeline.");

            try
            {
                ChatResponse response = await base.GetResponseAsync(messages, options, cancellationToken);
                stopwatch.Stop();

                int totalTokens = (int)(response.Usage?.TotalTokenCount ?? 0);
                int promptTokens = (int)(response.Usage?.InputTokenCount ?? 0);
                int completionTokens = (int)(response.Usage?.OutputTokenCount ?? 0);

                _logger.LogInformation(
                    "Pipeline completed successfully. Duration: {Duration}ms | Model: {Model} | Tokens: [Total: {Total}, Input: {Input}, Output: {Output}]",
                    stopwatch.ElapsedMilliseconds,
                    response.ModelId,
                    totalTokens,
                    promptTokens,
                    completionTokens);

                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Pipeline execution aborted after {Duration}ms. Error: {Message}",
                    stopwatch.ElapsedMilliseconds,
                    ex.Message);
                throw;
            }
        }
    }
}