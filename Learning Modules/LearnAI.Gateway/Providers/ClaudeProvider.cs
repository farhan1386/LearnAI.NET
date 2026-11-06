using LearnAI.Gateway.Enums;
using LearnAI.Gateway.Interfaces;
using LearnAI.Gateway.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using System;

namespace LearnAI.Gateway.Providers
{
    public class ClaudeProvider : IAIProvider
    {
        private readonly GatewaySettings _settings;

        public ClaudeProvider(IOptions<GatewaySettings> settings)
        {
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        }

        public ProviderType ProviderType => ProviderType.Claude;

        public IChatClient CreateClient(string? modelName = null)
        {
            string targetModel = string.IsNullOrWhiteSpace(modelName) ? _settings.Claude.DefaultModel : modelName;

            var credential = new System.ClientModel.ApiKeyCredential(_settings.Claude.ApiKey);

            var options = new OpenAI.OpenAIClientOptions { Endpoint = new Uri("https://anthropic.com") };

            var client = new OpenAI.Chat.ChatClient(targetModel, credential, options);
            return client.AsIChatClient();
        }
    }
}
