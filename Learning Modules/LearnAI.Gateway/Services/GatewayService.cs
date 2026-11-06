using LearnAI.Gateway.Enums;
using LearnAI.Gateway.Factories;
using LearnAI.Gateway.Interfaces;
using LearnAI.Gateway.Models;
using Microsoft.Extensions.AI;
using System.Diagnostics;

namespace LearnAI.Gateway.Services
{
    public class GatewayService : IGatewayService
    {
        private readonly AIProviderFactory _factory;

        public GatewayService(AIProviderFactory factory)
        {
            _factory = factory;
        }

        public async Task<string> ExecutePromptAsync(GatewayRequest request)
        {
            var provider = _factory.GetProvider(request.Provider);
            var client = provider.CreateClient(request.ModelName);

            var response = await client.GetResponseAsync(request.Prompt);
            return response.Text ?? string.Empty;
        }

        public async Task<List<ProviderMetric>> CompareProvidersAsync(string prompt, string? modelName = null)
        {
            var providersToTest = Enum.GetValues<ProviderType>();
            var tasks = providersToTest.Select(providerType => EvaluateSingleProviderAsync(providerType, prompt, modelName));

            var metricsArray = await Task.WhenAll(tasks);
            return metricsArray.ToList();
        }

        private async Task<ProviderMetric> EvaluateSingleProviderAsync(ProviderType providerType, string prompt, string? modelName)
        {
            var metric = new ProviderMetric
            {
                Provider = providerType,
                ModelUsed = modelName ?? "Default"
            };

            var stopwatch = Stopwatch.StartNew();
            try
            {
                var provider = _factory.GetProvider(providerType);
                var client = provider.CreateClient(modelName);

                var response = await client.GetResponseAsync(prompt);

                stopwatch.Stop();
                metric.IsSuccess = true;
                metric.Latency = stopwatch.Elapsed;
                metric.TokenCount = (int)(response.Usage?.TotalTokenCount ?? 0);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                metric.IsSuccess = false;
                metric.Latency = stopwatch.Elapsed;
                metric.ErrorMessage = ex.Message;
            }

            return metric;
        }
    }
}