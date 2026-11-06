using LearnAI.Gateway.Factories;
using LearnAI.Gateway.Interfaces;
using LearnAI.Gateway.Models;
using LearnAI.Gateway.Providers;
using LearnAI.Gateway.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LearnAI.Gateway.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAIGateway(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<GatewaySettings>(options =>
                configuration.GetSection("AIGateway").Bind(options));

            services.AddSingleton<IAIProvider, OpenAIProvider>();
            services.AddSingleton<IAIProvider, AzureOpenAIProvider>();
            services.AddSingleton<IAIProvider, OllamaProvider>();
            services.AddSingleton<IAIProvider, ClaudeProvider>();
            services.AddSingleton<IAIProvider, GeminiProvider>();

            services.AddSingleton<AIProviderFactory>();
            services.AddScoped<IGatewayService, GatewayService>();

            return services;
        }
    }
}
