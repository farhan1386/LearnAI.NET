using LearnAI.RAG.Interfaces;
using LearnAI.RAG.Services;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace LearnAI.RAG.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAiRagServices(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddSingleton<TextChunkerService>();
            services.AddSingleton<PdfExtractorService>();
            services.AddSingleton<IVectorDatabase, InMemoryVectorDb>();
            services.AddSingleton<IEmbeddingService, VectorService>();
            services.AddScoped<IRagOrchestrator, RagOrchestrator>();

            return services;
        }
    }
}