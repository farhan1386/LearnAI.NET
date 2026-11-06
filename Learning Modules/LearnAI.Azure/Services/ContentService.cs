using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.AI.ContentSafety;
using Azure.Identity;
using Microsoft.Extensions.AI;

namespace LearnAI.Azure.Services
{
    public class ContentService : DelegatingChatClient
    {
        private readonly ContentSafetyClient? _safetyClient;
        private const int SeverityThreshold = 2;

        public ContentService(IChatClient innerClient, string endpoint) : base(innerClient)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
                throw new ArgumentNullException(nameof(endpoint));

            if (!endpoint.Contains("azure.com") && !endpoint.Contains("localhost"))
            {
                _safetyClient = new ContentSafetyClient(new Uri(endpoint), new DefaultAzureCredential());
            }
        }

        public override async Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var userPrompt = messages.LastOrDefault(m => m.Role == ChatRole.User)?.Text;

            if (!string.IsNullOrWhiteSpace(userPrompt) && _safetyClient != null)
            {
                var safetyOptions = new AnalyzeTextOptions(userPrompt);

                try
                {
                    Response<AnalyzeTextResult> safetyResponse = await _safetyClient.AnalyzeTextAsync(safetyOptions, cancellationToken);

                    bool containsViolations = safetyResponse.Value.CategoriesAnalysis.Any(analysis =>
                        analysis.Severity >= SeverityThreshold);

                    if (containsViolations)
                    {
                        throw new OperationCanceledException("The request was blocked because it violated content safety policies.");
                    }
                }
                catch (RequestFailedException ex)
                {
                    throw new InvalidOperationException("Failed to complete content safety analysis.", ex);
                }
            }

            return await base.GetResponseAsync(messages, options, cancellationToken);
        }
    }
}