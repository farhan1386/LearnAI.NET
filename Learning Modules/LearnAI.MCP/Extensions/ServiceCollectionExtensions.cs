using Microsoft.Extensions.DependencyInjection;
using LearnAI.MCP.Interfaces;
using LearnAI.MCP.Client;
using LearnAI.MCP.Server;
using LearnAI.MCP.Resources;
using LearnAI.MCP.Tools;
using LearnAI.MCP.Prompts;
using LearnAI.MCP.Services;
using LearnAI.MCP.Models;

namespace LearnAI.MCP.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMcpInfrastructure(this IServiceCollection services, McpServerInfo serverInfo)
        {
            services.AddSingleton(serverInfo);

            services.AddSingleton<McpTracingService>();
            services.AddSingleton<IToolRegistry, ToolRegistry>();
            services.AddSingleton<IResourceRegistry, ResourceRegistry>();
            services.AddSingleton<PromptRegistry>();

            services.AddSingleton<IMcpClient, McpClient>();
            services.AddTransient<IMcpServer, McpServer>();

            services.AddScoped<McpService>();

            return services;
        }
    }
}
