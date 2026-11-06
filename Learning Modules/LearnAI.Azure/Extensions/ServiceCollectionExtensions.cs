using System;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using LearnAI.Azure.OpenAI;
using LearnAI.Azure.Services;
using LearnAI.Azure.Monitoring;
using LearnAI.Gateway.Models;

namespace LearnAI.Azure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAzureInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<GatewaySettings>(configuration.GetSection("AIGateway"));

            services.AddSingleton<IChatClient>(sp =>
            {
                var gatewaySection = configuration.GetSection("AIGateway");
                var azureSection = gatewaySection.GetSection("AzureOpenAI");

                var endpoint = azureSection["Endpoint"];
                var fallbackEndpoint = azureSection["FallbackEndpoint"];
                var safetyEndpoint = gatewaySection["AzureContentSafety:Endpoint"];

                var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
                var watchLogger = loggerFactory.CreateLogger<AzureAIMonitor>();
                var routeLogger = loggerFactory.CreateLogger<FailoverService>();

                var settingsOptions = sp.GetRequiredService<IOptions<GatewaySettings>>();
                var defaultModel = settingsOptions.Value.AzureOpenAI.DefaultDeployment;

                IChatClient primaryClient;
                IChatClient fallbackClient;

                // Secret-aware validation: Check if configurations or secrets are missing/empty
                if (string.IsNullOrWhiteSpace(endpoint) || endpoint.Contains("azure.com"))
                {
                    var ollamaEndpoint = gatewaySection["Ollama:Endpoint"] ?? "http://localhost:11434";
                    var ollamaModel = gatewaySection["Ollama:DefaultModel"] ?? "phi4";

                    primaryClient = new OllamaChatClient(new Uri(ollamaEndpoint), ollamaModel);
                    fallbackClient = new OllamaChatClient(new Uri(ollamaEndpoint), ollamaModel);
                }
                else
                {
                    var baseProvider = new AzureOpenAIProvider(settingsOptions);
                    primaryClient = baseProvider.CreateClient();

                    // Managed Identity / Key Vault credentials provider
                    var credential = new DefaultAzureCredential();
                    var fallbackTargetUri = new Uri(fallbackEndpoint ?? endpoint);
                    var fallbackAzureClient = new AzureOpenAIClient(fallbackTargetUri, credential);

                    fallbackClient = fallbackAzureClient.GetChatClient(defaultModel).AsIChatClient();
                }

                var activeSafetyUri = string.IsNullOrWhiteSpace(safetyEndpoint) || safetyEndpoint.Contains("azure.com")
                    ? "https://localhost"
                    : safetyEndpoint;

                return primaryClient
                    .AsBuilder()
                    .Use(inner => new ContentService(inner, activeSafetyUri))
                    .Use(inner => new FailoverService(inner, fallbackClient, routeLogger))
                    .Use(inner => new AzureAIMonitor(inner, watchLogger))
                    .Build();
            });

            return services;
        }
    }
}