using LearnAI.MCP.Models;
using LearnAI.MCP.Prompts;
using LearnAI.MCP.Resources;
using LearnAI.MCP.Tools;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LearnAI.MCP.Interfaces
{
    public interface IMcpClient
    {
        Task<IEnumerable<McpTool>> ListToolsAsync(CancellationToken cancellationToken = default);

        Task<string> ExecuteToolAsync(
            string serverName,
            string toolName,
            string argumentsJson,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<McpResource>> ListResourcesAsync(CancellationToken cancellationToken = default);

        Task<string> ReadResourceAsync(
            string serverName,
            string resourceUri,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<McpPrompt>> ListPromptsAsync(CancellationToken cancellationToken = default);

        Task<McpPrompt> GetPromptAsync(
            string serverName,
            string promptName,
            Dictionary<string, string> arguments,
            CancellationToken cancellationToken = default);
    }
}
