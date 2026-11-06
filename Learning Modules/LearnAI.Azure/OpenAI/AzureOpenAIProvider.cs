using System;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using LearnAI.Gateway.Models;

namespace LearnAI.Azure.OpenAI
{
    public class AzureOpenAIProvider
    {
        private readonly GatewaySettings _settings;

        public AzureOpenAIProvider(IOptions<GatewaySettings> settings)
        {
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        }

        public IChatClient CreateClient(string? deploymentName = null)
        {
            string targetDeployment = string.IsNullOrWhiteSpace(deploymentName)
                ? _settings.AzureOpenAI.DefaultDeployment
                : deploymentName;

            var azureClient = new AzureOpenAIClient(new Uri(_settings.AzureOpenAI.Endpoint), new DefaultAzureCredential());
            return azureClient.GetChatClient(targetDeployment).AsIChatClient();
        }
    }
}
