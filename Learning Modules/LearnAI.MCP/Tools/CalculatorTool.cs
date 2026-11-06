using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LearnAI.MCP.Models;

namespace LearnAI.MCP.Tools
{
    public static class CalculatorTool
    {
        public static McpTool GetMetadata()
        {
            return new McpTool
            {
                Name = "calculate",
                Description = "Performs arithmetic calculations (add, subtract, multiply, divide).",
                InputSchema = new Dictionary<string, object>
                {
                    { "type", "object" },
                    { "properties", new Dictionary<string, object>
                        {
                            { "a", new Dictionary<string, string> { { "type", "number" } } },
                            { "b", new Dictionary<string, string> { { "type", "number" } } },
                            { "op", new Dictionary<string, object> { { "type", "string" }, { "enum", new[] { "add", "subtract", "multiply", "divide" } } } }
                        }
                    },
                    { "required", new[] { "a", "b", "op" } }
                }
            };
        }

        public static Task<string> HandleExecutionAsync(string argumentsJson, CancellationToken cancellationToken = default)
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var root = doc.RootElement;

            if (!root.TryGetProperty("a", out var aProp) ||
                !root.TryGetProperty("b", out var bProp) ||
                !root.TryGetProperty("op", out var opProp))
            {
                return Task.FromResult(JsonSerializer.Serialize(new { error = "Missing required execution arguments: 'a', 'b', or 'op'." }));
            }

            double a = aProp.GetDouble();
            double b = bProp.GetDouble();
            string op = opProp.GetString() ?? string.Empty;

            double result = op switch
            {
                "add" => a + b,
                "subtract" => a - b,
                "multiply" => a * b,
                "divide" => b != 0 ? a / b : throw new DivideByZeroException("Cannot divide by zero."),
                _ => throw new ArgumentException($"Unknown operator: {op}")
            };

            return Task.FromResult(JsonSerializer.Serialize(new { value = result }));
        }
    }
}