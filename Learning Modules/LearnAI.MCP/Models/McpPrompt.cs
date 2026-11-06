using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LearnAI.MCP.Models
{
    public record McpPrompt
    {
        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; init; } = string.Empty;

        [JsonPropertyName("arguments")]
        public List<McpPromptArgument> Arguments { get; init; } = new();
    }

    public record McpPromptArgument
    {
        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; init; } = string.Empty;

        [JsonPropertyName("required")]
        public bool Required { get; init; } = false;
    }
}
