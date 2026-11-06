using Microsoft.Extensions.DependencyInjection;
using LearnAI.Tools.Interfaces;
using LearnAI.Tools.Middleware;
using LearnAI.Tools.Services;
using LearnAI.Tools.Calculator;
using LearnAI.Tools.CodeReview;
using LearnAI.Tools.FileTools;
using LearnAI.Tools.Weather;

namespace LearnAI.Tools.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLearnAIToolsEngine(this IServiceCollection services)
        {
            services.AddSingleton<IJsonSchemaBuilder, JsonSchemaBuilder>();
            services.AddSingleton<StructuredOutputService>();

            services.AddSingleton<IToolMiddleware, ToolLoggingMiddleware>();
            services.AddSingleton<IToolMiddleware, ToolSandboxMiddleware>();

            services.AddSingleton<CalculatorService>();
            services.AddSingleton<ReviewEngine>();
            services.AddSingleton<FileService>();
            services.AddSingleton<WeatherService>();

            services.AddSingleton<IToolExecutionEngine>(provider =>
            {
                var middlewares = provider.GetServices<IToolMiddleware>();
                var engine = new ToolExecutionEngine(middlewares);
                var schemaBuilder = provider.GetRequiredService<IJsonSchemaBuilder>();

                var calculator = provider.GetRequiredService<CalculatorService>();
                foreach (var method in typeof(CalculatorService).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly))
                {
                    engine.RegisterTool(new MethodTool(calculator, method, schemaBuilder));
                }

                var codeReview = provider.GetRequiredService<ReviewEngine>();
                foreach (var method in typeof(ReviewEngine).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly))
                {
                    engine.RegisterTool(new MethodTool(codeReview, method, schemaBuilder));
                }

                var fileTools = provider.GetRequiredService<FileService>();
                foreach (var method in typeof(FileService).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly))
                {
                    engine.RegisterTool(new MethodTool(fileTools, method, schemaBuilder));
                }

                var weather = provider.GetRequiredService<WeatherService>();
                foreach (var method in typeof(WeatherService).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly))
                {
                    engine.RegisterTool(new MethodTool(weather, method, schemaBuilder));
                }

                return engine;
            });

            return services;
        }
    }
}