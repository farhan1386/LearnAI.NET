using System;
using System.Text.Json.Serialization;

namespace LearnAI.MCP.Models
{
    public record McpServerInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        [JsonPropertyName("version")]
        public string Version { get; init; } = string.Empty;

        [JsonPropertyName("protocolVersion")]
        public string ProtocolVersion { get; init; } = "2024-11-05";
    }
}