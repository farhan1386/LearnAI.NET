using Microsoft.Extensions.DependencyInjection;
using LearnAI.Agents.Interfaces;
using LearnAI.Agents.Services;
using LearnAI.Agents.ResearchAgent;
using LearnAI.Agents.WriterAgent;
using LearnAI.Agents.ReviewAgent;

namespace LearnAI.Agents.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAgentInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<ResearchAgentService>();
            services.AddScoped<WriterAgentService>();
            services.AddScoped<ReviewAgentService>();

            services.AddScoped<IAgentPlanner, AgentPlanner>();
            services.AddScoped<IAgentOrchestrator, AgentOrchestrator>();

            return services;
        }
    }
}
