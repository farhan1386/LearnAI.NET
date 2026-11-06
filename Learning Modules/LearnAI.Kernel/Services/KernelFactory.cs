using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using LearnAI.Kernel.Diagnostics;

namespace LearnAI.Kernel.Services
{
    public class KernelFactory
    {
        private readonly IConfiguration _configuration;

        public KernelFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Microsoft.SemanticKernel.Kernel CreateKernel()
        {
            var builder = Microsoft.SemanticKernel.Kernel.CreateBuilder();

            string openAiKey = _configuration["AIProviders:OpenAI:ApiKey"] ?? string.Empty;
            string modelId = _configuration["AIProviders:OpenAI:ModelId"] ?? "gpt-4o";

            var tracingClient = new HttpClient(new PluginTracingHandler());

            if (!string.IsNullOrEmpty(openAiKey))
            {
                builder.AddOpenAIChatCompletion(modelId, openAiKey, httpClient: tracingClient);
                builder.AddOpenAITextEmbeddingGeneration("text-embedding-3-small", openAiKey, httpClient: tracingClient);
            }

            builder.Plugins.AddFromType<Plugins.TimePlugin>("TimePlugin");
            builder.Plugins.AddFromType<Plugins.MathPlugin>("MathPlugin");

            return builder.Build();
        }
    }
}