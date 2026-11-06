using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LearnAI.MCP.Interfaces;
using LearnAI.MCP.Models;

namespace LearnAI.MCP.Tools
{
    public class ToolRegistry : IToolRegistry
    {
        private readonly ConcurrentDictionary<string, (McpTool Metadata, Func<string, CancellationToken, Task<string>> Handler)> _tools = new();

        public void RegisterTool(McpTool tool, Func<string, CancellationToken, Task<string>> handler)
        {
            if (tool == null) throw new ArgumentNullException(nameof(tool));
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            _tools[tool.Name] = (tool, handler);
        }

        public Task<IEnumerable<McpTool>> GetToolsAsync(CancellationToken cancellationToken = default)
        {
            var metadataList = new List<McpTool>();
            foreach (var item in _tools.Values)
            {
                metadataList.Add(item.Metadata);
            }
            return Task.FromResult<IEnumerable<McpTool>>(metadataList);
        }

        public async Task<string> ExecuteToolAsync(string name, string argumentsJson, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Tool name cannot be null or empty.", nameof(name));

            if (!_tools.TryGetValue(name, out var mapping))
            {
                throw new KeyNotFoundException($"Tool with name '{name}' was not found in the registry.");
            }

            return await mapping.Handler(argumentsJson, cancellationToken);
        }
    }
}