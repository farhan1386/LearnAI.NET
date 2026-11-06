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
    public interface IMcpServer
    {
        McpServerInfo ServerInfo { get; }

        Task StartAsync(CancellationToken cancellationToken = default);

        Task StopAsync(CancellationToken cancellationToken = default);

        Task<IEnumerable<McpTool>> HandleListToolsAsync(CancellationToken cancellationToken = default);

        Task<string> HandleCallToolAsync(string name, string argumentsJson, CancellationToken cancellationToken = default);

        Task<IEnumerable<McpResource>> HandleListResourcesAsync(CancellationToken cancellationToken = default);

        Task<string> HandleReadResourceAsync(string uri, CancellationToken cancellationToken = default);

        Task<IEnumerable<McpPrompt>> HandleListPromptsAsync(CancellationToken cancellationToken = default);

        Task<McpPrompt> HandleGetPromptAsync(string name, Dictionary<string, string> arguments, CancellationToken cancellationToken = default);
    }
}
