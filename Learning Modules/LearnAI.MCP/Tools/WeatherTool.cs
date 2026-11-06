using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LearnAI.MCP.Models;

namespace LearnAI.MCP.Tools
{
    public static class WeatherTool
    {
        public static McpTool GetMetadata()
        {
            return new McpTool
            {
                Name = "get_weather",
                Description = "Retrieves current weather details for a specific city.",
                InputSchema = new Dictionary<string, object>
                {
                    { "type", "object" },
                    { "properties", new Dictionary<string, object>
                        {
                            { "city", new Dictionary<string, string> { { "type", "string" } } }
                        }
                    },
                    { "required", new[] { "city" } }
                }
            };
        }

        public static Task<string> HandleExecutionAsync(string argumentsJson, CancellationToken cancellationToken = default)
        {
            using var doc = JsonDocument.Parse(argumentsJson);
            var root = doc.RootElement;

            if (!root.TryGetProperty("city", out var cityProp))
            {
                return Task.FromResult(JsonSerializer.Serialize(new { error = "Missing required argument: 'city'." }));
            }

            var city = cityProp.GetString() ?? "Unknown";

            var mockResponse = new
            {
                location = city,
                temperature = 22.5,
                unit = "celsius",
                condition = "Sunny"
            };

            return Task.FromResult(JsonSerializer.Serialize(mockResponse));
        }
    }
}