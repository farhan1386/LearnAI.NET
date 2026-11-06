using LearnAI.Gateway.Enums;
using LearnAI.Gateway.Interfaces;

namespace LearnAI.Gateway.Factories
{
    public class AIProviderFactory
    {
        private readonly IEnumerable<IAIProvider> _providers;

        public AIProviderFactory(IEnumerable<IAIProvider> providers)
        {
            _providers = providers ?? throw new ArgumentNullException(nameof(providers));
        }

        public IAIProvider GetProvider(ProviderType providerType)
        {
            var provider = _providers.FirstOrDefault(p => p.ProviderType == providerType);
            if (provider == null)
            {
                throw new KeyNotFoundException($"No AI provider registration found matching type: {providerType}");
            }
            return provider;
        }
    }
}
