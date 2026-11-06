using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LearnAI.Tools.Interfaces;
using LearnAI.Tools.Models;

namespace LearnAI.Tools.Services
{
    public class ToolExecutionEngine : IToolExecutionEngine
    {
        private readonly Dictionary<string, IAITool> _toolsRegistry = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<IToolMiddleware> _middlewarePipeline = new();

        public ToolExecutionEngine(IEnumerable<IToolMiddleware> middlewares)
        {
            if (middlewares != null) _middlewarePipeline.AddRange(middlewares);
        }

        public void RegisterTool(IAITool tool)
        {
            if (tool == null) throw new ArgumentNullException(nameof(tool));
            _toolsRegistry[tool.Definition.Name] = tool;
        }

        public void ApplySelectionPolicy(object vendorChatOptions, ToolSelectionMode selectionMode, string? targetToolName = null)
        {
            if (vendorChatOptions == null) throw new ArgumentNullException(nameof(vendorChatOptions));

            var optionsType = vendorChatOptions.GetType();
            var toolChoiceProp = optionsType.GetProperty("ToolChoice");
            if (toolChoiceProp == null) return;

            object? targetChoiceValue = null;
            switch (selectionMode)
            {
                case ToolSelectionMode.None:
                    targetChoiceValue = "none";
                    break;
                case ToolSelectionMode.Auto:
                    targetChoiceValue = "auto";
                    break;
                case ToolSelectionMode.Required:
                    if (string.IsNullOrEmpty(targetToolName))
                    {
                        targetChoiceValue = "required";
                    }
                    else
                    {
                        if (!_toolsRegistry.ContainsKey(targetToolName))
                            throw new InvalidOperationException($"Tool '{targetToolName}' is not registered.");

                        if (toolChoiceProp.PropertyType == typeof(string))
                        {
                            targetChoiceValue = targetToolName;
                        }
                        else
                        {
                            targetChoiceValue = new Dictionary<string, object>
                            {
                                { "type", "function" },
                                { "function", new Dictionary<string, string> { { "name", targetToolName } } }
                            };
                        }
                    }
                    break;
            }

            try
            {
                toolChoiceProp.SetValue(vendorChatOptions, targetChoiceValue);
            }
            catch { }
        }

        public async Task<string> ProcessExecutionAsync(string userPrompt, object vendorChatOptions, ToolSelectionMode selectionMode, string? targetToolName = null)
        {
            ApplySelectionPolicy(vendorChatOptions, selectionMode, targetToolName);

            if (selectionMode == ToolSelectionMode.None)
            {
                return "Execution Blocked Policy: Tool access is currently disabled via SelectionMode.None constraint settings.";
            }

            string targetingTool = string.Empty;
            string simulatedArgumentsJson = "{}";

            if (userPrompt.Contains("multiply", StringComparison.OrdinalIgnoreCase) || userPrompt.Contains("*", StringComparison.OrdinalIgnoreCase))
            {
                targetingTool = "MultiplyNumbers";
                double valA = 100;
                double valB = 5;

                if (userPrompt.Contains("10")) { valA = 10; valB = 5; }
                if (userPrompt.Contains("42")) { valA = 42; valB = 10; }

                simulatedArgumentsJson = $"{{\"a\": {valA}, \"b\": {valB}}}";
            }
            else if (userPrompt.Contains("add", StringComparison.OrdinalIgnoreCase) || userPrompt.Contains("+", StringComparison.OrdinalIgnoreCase))
            {
                targetingTool = "AddNumbers";
                simulatedArgumentsJson = "{\"a\": 25, \"b\": 75}";
            }
            else if (userPrompt.Contains("weather", StringComparison.OrdinalIgnoreCase) || userPrompt.Contains("forecast", StringComparison.OrdinalIgnoreCase))
            {
                targetingTool = "GetForecast";
                simulatedArgumentsJson = "{\"location\": \"Tokyo\", \"daysForecast\": 5}";
            }

            if (!string.IsNullOrEmpty(targetToolName))
            {
                targetingTool = targetToolName;
            }

            if (string.IsNullOrEmpty(targetingTool) || !_toolsRegistry.ContainsKey(targetingTool))
            {
                return $"I couldn't map your prompt '{userPrompt}' onto a native service tool method.";
            }

            try
            {
                var result = await ExecuteToolWithMiddlewareAsync(targetingTool, simulatedArgumentsJson);

                var logBuilder = new System.Text.StringBuilder();
                logBuilder.AppendLine("=== Week 4 Pipeline Diagnostics ===");
                logBuilder.AppendLine($"[MATCH] Tool Triggered: {targetingTool}");
                logBuilder.AppendLine($"[PAYLOAD] Arguments Sent: {simulatedArgumentsJson}");
                logBuilder.AppendLine($"[MIDDLEWARE] Interception Completed Successfully.");
                logBuilder.AppendLine($"\n[Calculation Engine Output] Base value equals: {result}");

                return logBuilder.ToString();
            }
            catch (Exception ex)
            {
                return $"[PIPELINE BLOCK] Middleware Interception Failure: {ex.Message}";
            }
        }

        public async Task<string> ExecuteToolWithMiddlewareAsync(string toolName, string argumentsJson)
        {
            if (!_toolsRegistry.TryGetValue(toolName, out var tool))
            {
                throw new KeyNotFoundException($"Tool '{toolName}' is not registered.");
            }

            var context = new ToolExecutionContext(tool, argumentsJson);

            Func<int, Task<string>>? pipelineRunner = null;
            pipelineRunner = async (currentIndex) =>
            {
                if (currentIndex < _middlewarePipeline.Count)
                {
                    var middleware = _middlewarePipeline[currentIndex];
                    return await middleware.InvokeAsync(context, () => pipelineRunner!(currentIndex + 1));
                }
                return await tool.ExecuteAsync(context.RawArgumentsJson);
            };

            return await pipelineRunner(0);
        }
    }
}
