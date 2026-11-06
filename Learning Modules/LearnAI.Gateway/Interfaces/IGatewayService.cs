using LearnAI.Gateway.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LearnAI.Gateway.Interfaces
{
    public interface IGatewayService
    {
        Task<string> ExecutePromptAsync(GatewayRequest request);
        Task<List<ProviderMetric>> CompareProvidersAsync(string prompt, string? modelName = null);
    }
}