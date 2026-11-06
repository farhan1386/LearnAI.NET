using LearnAI.Gateway.Enums;
using LearnAI.Gateway.Interfaces;
using LearnAI.Gateway.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using System;

namespace LearnAI.Gateway.Providers
{
    public class OllamaProvider : IAIProvider
    {
        private readonly GatewaySettings _settings;

        public OllamaProvider(IOptions<GatewaySettings> settings)
        {
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        }

        public ProviderType ProviderType => ProviderType.Ollama;

        public IChatClient CreateClient(string? modelName = null)
        {
            string targetModel = string.IsNullOrWhiteSpace(modelName) ? _settings.Ollama.DefaultModel : modelName;
            return new OllamaChatClient(new Uri(_settings.Ollama.Endpoint), targetModel);
        }
    }
}
