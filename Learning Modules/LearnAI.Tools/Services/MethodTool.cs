using LearnAI.Tools.Interfaces;
using LearnAI.Tools.Models;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;

namespace LearnAI.Tools.Services
{
    public class MethodTool : IAITool
    {
        private readonly object _targetInstance;
        private readonly MethodInfo _methodInfo;
        public ToolDefinition Definition { get; }

        public MethodTool(object targetInstance, MethodInfo methodInfo, IJsonSchemaBuilder schemaBuilder)
        {
            _targetInstance = targetInstance ?? throw new ArgumentNullException(nameof(targetInstance));
            _methodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
            if (schemaBuilder == null) throw new ArgumentNullException(nameof(schemaBuilder));

            var descAttr = _methodInfo.GetCustomAttribute<DescriptionAttribute>();
            var description = descAttr?.Description ?? $"Executes the {_methodInfo.Name} method.";

            Definition = new ToolDefinition
            {
                Name = _methodInfo.Name,
                Description = description,
                ParametersSchemaJson = schemaBuilder.BuildSchema(_methodInfo)
            };
        }

        public async Task<string> ExecuteAsync(string argumentsJson)
        {
            try
            {
                var parameters = _methodInfo.GetParameters();
                var invokeArgs = new object?[parameters.Length];

                if (parameters.Length > 0 && !string.IsNullOrWhiteSpace(argumentsJson))
                {
                    using var doc = JsonDocument.Parse(argumentsJson);
                    var root = doc.RootElement;

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var param = parameters[i];
                        if (string.IsNullOrEmpty(param.Name)) continue;

                        if (root.TryGetProperty(param.Name, out var element))
                        {
                            var rawVal = element.GetRawText();
                            invokeArgs[i] = JsonSerializer.Deserialize(rawVal, param.ParameterType);
                        }
                        else if (param.HasDefaultValue)
                        {
                            invokeArgs[i] = param.DefaultValue;
                        }
                        else
                        {
                            throw new InvalidOperationException($"Missing required parameter: '{param.Name}'");
                        }
                    }
                }

                var result = _methodInfo.Invoke(_targetInstance, invokeArgs);

                if (result is Task task)
                {
                    await task.ConfigureAwait(false);
                    var resultProperty = task.GetType().GetProperty("Result");
                    var taskResult = resultProperty?.GetValue(task);
                    return taskResult?.ToString() ?? string.Empty;
                }

                return result?.ToString() ?? string.Empty;
            }
            catch (TargetInvocationException ex)
            {
                throw new InvalidOperationException($"Execution failed inside {_methodInfo.Name}: {ex.InnerException?.Message}", ex.InnerException);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to execute tool: {ex.Message}", ex);
            }
        }
    }
}