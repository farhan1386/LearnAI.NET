using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LearnAI.MCP.Models;

namespace LearnAI.MCP.Interfaces
{
    public interface IToolRegistry
    {
        Task<IEnumerable<McpTool>> GetToolsAsync(CancellationToken cancellationToken = default);

        Task<string> ExecuteToolAsync(string name, string argumentsJson, CancellationToken cancellationToken = default);
    }
}