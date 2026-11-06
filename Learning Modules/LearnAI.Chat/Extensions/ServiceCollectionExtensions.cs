using LearnAI.Chat.Interfaces;
using LearnAI.Chat.Services;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace LearnAI.Chat.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAiChatServices(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddSingleton<ConversationMemoryService>();
            services.AddSingleton<TokenService>();
            services.AddSingleton<IChatClientFactory, OllamaChatClientFactory>();
            services.AddScoped<ChatService>();

            return services;
        }
    }
}