using LearnAI.Gateway.Enums;

namespace LearnAI.Gateway.Models
{
    public class GatewayRequest
    {
        public ProviderType Provider { get; set; }
        public string? ModelName { get; set; }
        public string? Prompt { get; set; } 
    }
}
