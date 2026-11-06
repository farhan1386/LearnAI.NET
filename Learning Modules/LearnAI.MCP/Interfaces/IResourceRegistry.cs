using LearnAI.MCP.Models;
using LearnAI.MCP.Resources;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LearnAI.MCP.Interfaces
{
    public interface IResourceRegistry
    {
        Task<IEnumerable<McpResource>> GetResourcesAsync(CancellationToken cancellationToken = default);

        Task<string> ReadResourceAsync(string uri, CancellationToken cancellationToken = default);
    }
}
