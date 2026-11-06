using CommunityToolkit.VectorData.InMemory;
using LearnAI.Kernel.Orchestration;
using LearnAI.Kernel.Services;
using LearnAI.Kernel.Workflows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;

namespace LearnAI.Kernel.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSemanticKernelInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<KernelFactory>();

            services.AddSingleton<VectorStore, InMemoryVectorStore>();

            services.AddTransient<ITextEmbeddingGenerationService>(sp =>
            {
                var factory = sp.GetRequiredService<KernelFactory>();
                var kernel = factory.CreateKernel();
                return kernel.GetRequiredService<ITextEmbeddingGenerationService>();
            });

            services.AddScoped<KernelMemoryService>();

            services.AddScoped<MultiStepPlanner>();

            services.AddScoped<StepWiseWorkflowHandler>();

            services.AddTransient<Microsoft.SemanticKernel.Kernel>(sp =>
            {
                var factory = sp.GetRequiredService<KernelFactory>();
                return factory.CreateKernel();
            });

            return services;
        }
    }
}