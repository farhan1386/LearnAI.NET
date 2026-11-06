using LearnAI.Gateway.Enums;

namespace LearnAI.Gateway.Models
{
    public class ProviderConfiguration
    {
        public ProviderType ProviderType { get; set; }
        public string? DefaultModel { get; set; }
    }
}
