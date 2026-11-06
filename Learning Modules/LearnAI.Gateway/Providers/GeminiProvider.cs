using LearnAI.Gateway.Enums;
using LearnAI.Gateway.Interfaces;
using LearnAI.Gateway.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Mscc.GenerativeAI.Microsoft;

namespace LearnAI.Gateway.Providers
{
    public class GeminiProvider : IAIProvider
    {
        private readonly GatewaySettings _settings;

        public GeminiProvider(IOptions<GatewaySettings> settings)
        {
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        }

        public ProviderType ProviderType => ProviderType.Gemini;

        public IChatClient CreateClient(string? modelName = null)
        {
            string targetModel = string.IsNullOrWhiteSpace(modelName) ? _settings.Gemini.DefaultModel : modelName;

            return new GeminiChatClient(apiKey: _settings.Gemini.ApiKey, model: targetModel);
        }
    }
}
