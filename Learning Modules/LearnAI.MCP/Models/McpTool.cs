using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LearnAI.MCP.Models
{
    public record McpTool
    {
        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; init; } = string.Empty;

        [JsonPropertyName("inputSchema")]
        public Dictionary<string, object> InputSchema { get; init; } = new();
    }
}
