using System;
using System.Text.Json.Serialization;

namespace LearnAI.MCP.Models
{
    public record McpResource
    {
        [JsonPropertyName("uri")]
        public string Uri { get; init; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; init; } = string.Empty;

        [JsonPropertyName("mimeType")]
        public string MimeType { get; init; } = "text/plain";
    }
}
