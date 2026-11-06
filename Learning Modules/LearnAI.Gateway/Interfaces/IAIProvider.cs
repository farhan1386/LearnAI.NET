using Microsoft.Extensions.AI;
using LearnAI.Gateway.Enums;

namespace LearnAI.Gateway.Interfaces
{
    public interface IAIProvider
    {
        ProviderType ProviderType { get; }
        IChatClient CreateClient(string? modelName = null);
    }
}
