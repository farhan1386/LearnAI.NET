using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace LearnAI.Azure.Services
{
    public class FailoverService : DelegatingChatClient
    {
        private readonly IChatClient _fallbackClient;
        private readonly ILogger<FailoverService> _logger;

        public FailoverService(IChatClient primaryClient, IChatClient fallbackClient, ILogger<FailoverService> logger)
            : base(primaryClient)
        {
            _fallbackClient = fallbackClient ?? throw new ArgumentNullException(nameof(fallbackClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await base.GetResponseAsync(messages, options, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Primary Azure OpenAI endpoint failed. Routing execution to regional fallback path.");

                try
                {
                    return await _fallbackClient.GetResponseAsync(messages, options, cancellationToken);
                }
                catch (Exception fallbackEx)
                {
                    _logger.LogCritical(fallbackEx, "All configured Azure OpenAI routing profiles failed execution.");
                    throw;
                }
            }
        }
    }
}