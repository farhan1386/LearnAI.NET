using Azure.AI.OpenAI;
using Azure.Identity;
using LearnAI.Gateway.Enums;
using LearnAI.Gateway.Interfaces;
using LearnAI.Gateway.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using System;

namespace LearnAI.Gateway.Providers
{
    public class AzureOpenAIProvider : IAIProvider
    {
        private readonly GatewaySettings _settings;

        public AzureOpenAIProvider(IOptions<GatewaySettings> settings)
        {
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        }

        public ProviderType ProviderType => ProviderType.AzureOpenAI;

        public IChatClient CreateClient(string? modelName = null)
        {
            string deploymentName = string.IsNullOrWhiteSpace(modelName) ? _settings.AzureOpenAI.DefaultDeployment : modelName;
            var azureClient = new AzureOpenAIClient(new Uri(_settings.AzureOpenAI.Endpoint), new DefaultAzureCredential());
            return azureClient.GetChatClient(deploymentName).AsIChatClient();
        }
    }
}
