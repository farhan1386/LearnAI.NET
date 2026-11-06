using LearnAI.Gateway.Enums;
using LearnAI.Gateway.Interfaces;
using LearnAI.Gateway.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;

namespace LearnAI.Gateway.Providers
{
    public class OpenAIProvider : IAIProvider
    {
        private readonly GatewaySettings _settings;

        public OpenAIProvider(IOptions<GatewaySettings> settings)
        {
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        }

        public ProviderType ProviderType => ProviderType.OpenAI;

        public IChatClient CreateClient(string? modelName = null)
        {
            string targetModel = string.IsNullOrWhiteSpace(modelName) ? _settings.OpenAI.DefaultModel : modelName;

            var credential = new System.ClientModel.ApiKeyCredential(_settings.OpenAI.ApiKey);
            var chatClient = new OpenAI.Chat.ChatClient(targetModel, credential);

            return chatClient.AsIChatClient();
        }
    }
}
