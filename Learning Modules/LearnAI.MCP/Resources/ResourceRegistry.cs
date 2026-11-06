using LearnAI.MCP.Interfaces;
using LearnAI.MCP.Models;
using System.Collections.Concurrent;

namespace LearnAI.MCP.Resources
{
    public class ResourceRegistry : IResourceRegistry
    {
        private readonly ConcurrentDictionary<string, (McpResource Metadata, Func<string, Task<string>> Resolver)> _resources = new();

        public void RegisterResource(McpResource resource, Func<string, Task<string>> resolver)
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));

            _resources[resource.Uri] = (resource, resolver);
        }

        public Task<IEnumerable<McpResource>> GetResourcesAsync(CancellationToken cancellationToken = default)
        {
            var metadataList = new List<McpResource>();
            foreach (var item in _resources.Values)
            {
                metadataList.Add(item.Metadata);
            }
            return Task.FromResult<IEnumerable<McpResource>>(metadataList);
        }

        public async Task<string> ReadResourceAsync(string uri, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(uri)) throw new ArgumentException("URI cannot be null or empty.", nameof(uri));

            if (!_resources.TryGetValue(uri, out var mapping))
            {
                throw new KeyNotFoundException($"Resource with URI '{uri}' was not found in the registry.");
            }

            return await mapping.Resolver(uri);
        }
    }
}