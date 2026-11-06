using LearnAI.MCP.Interfaces;
using LearnAI.MCP.Models;
using LearnAI.MCP.Services;
using System.Collections.Concurrent;
using System.Text.Json;

namespace LearnAI.MCP.Client
{
    public class McpClient : IMcpClient
    {
        private readonly ConcurrentDictionary<string, (Stream RequestStream, Stream ResponseStream)> _serverPool = new();
        private readonly ConcurrentDictionary<string, TaskCompletionSource<JsonDocument>> _pendingRequests = new();
        private readonly McpTracingService _tracer;
        private long _requestId = 0;

        public McpClient(McpTracingService tracer)
        {
            _tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));
        }

        public void RegisterServerTransport(string serverName, Stream requestStream, Stream responseStream)
        {
            if (string.IsNullOrEmpty(serverName)) throw new ArgumentException("Server name cannot be empty.", nameof(serverName));
            _serverPool[serverName] = (requestStream, responseStream);
            _tracer.LogTrace($"[McpClient] Transport route configured for server connection pool: '{serverName}'.");
        }

        public async Task StartListeningAsync(string serverName, CancellationToken cancellationToken = default)
        {
            if (!_serverPool.TryGetValue(serverName, out var streams))
            {
                throw new KeyNotFoundException($"Server '{serverName}' transport streams are not found.");
            }

            var reader = new StreamReader(streams.ResponseStream);

            _ = Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var line = await reader.ReadLineAsync(cancellationToken);
                        if (string.IsNullOrEmpty(line)) break;

                        using var doc = JsonDocument.Parse(line);
                        var root = doc.RootElement;

                        if (root.TryGetProperty("id", out var idProp))
                        {
                            var idString = idProp.ToString();
                            if (_pendingRequests.TryRemove(idString, out var tcs))
                            {
                                tcs.SetResult(JsonDocument.Parse(line));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _tracer.LogError($"[McpClient] Error reading response pipeline stream for '{serverName}': {ex.Message}");
                        break;
                    }
                }
            }, cancellationToken);

            await Task.CompletedTask;
        }

        public async Task<IEnumerable<McpTool>> ListToolsAsync(CancellationToken cancellationToken = default)
        {
            var aggregatedTools = new List<McpTool>();

            foreach (var serverName in _serverPool.Keys)
            {
                try
                {
                    var response = await SendRequestAsync(serverName, "tools/list", null, cancellationToken);
                    var root = response.RootElement;

                    if (root.TryGetProperty("result", out var resultProp) && resultProp.TryGetProperty("tools", out var toolsProp))
                    {
                        var tools = JsonSerializer.Deserialize<IEnumerable<McpTool>>(toolsProp.GetRawText());
                        if (tools != null) aggregatedTools.AddRange(tools);
                    }
                }
                catch (Exception ex)
                {
                    _tracer.LogError($"[McpClient] Failed listing tools from server '{serverName}': {ex.Message}");
                }
            }

            return aggregatedTools;
        }

        public async Task<string> ExecuteToolAsync(string serverName, string toolName, string argumentsJson, CancellationToken cancellationToken = default)
        {
            var parameters = new Dictionary<string, object>
            {
                { "name", toolName },
                { "arguments", JsonDocument.Parse(argumentsJson).RootElement }
            };

            var response = await SendRequestAsync(serverName, "tools/call", parameters, cancellationToken);
            var root = response.RootElement;

            if (root.TryGetProperty("error", out var errorProp))
            {
                throw new InvalidOperationException($"Protocol JSON-RPC Error: {errorProp.GetRawText()}");
            }

            if (root.TryGetProperty("result", out var resultProp))
            {
                return resultProp.GetString() ?? string.Empty;
            }

            throw new InvalidOperationException("Malformed result structure received from the remote tool handler.");
        }

        public async Task<IEnumerable<McpResource>> ListResourcesAsync(CancellationToken cancellationToken = default)
        {
            var aggregatedResources = new List<McpResource>();

            foreach (var serverName in _serverPool.Keys)
            {
                try
                {
                    var response = await SendRequestAsync(serverName, "resources/list", null, cancellationToken);
                    var root = response.RootElement;

                    if (root.TryGetProperty("result", out var resultProp) && resultProp.TryGetProperty("resources", out var resourcesProp))
                    {
                        var resources = JsonSerializer.Deserialize<IEnumerable<McpResource>>(resourcesProp.GetRawText());
                        if (resources != null) aggregatedResources.AddRange(resources);
                    }
                }
                catch (Exception ex)
                {
                    _tracer.LogError($"[McpClient] Failed listing resources from server '{serverName}': {ex.Message}");
                }
            }

            return aggregatedResources;
        }

        public async Task<string> ReadResourceAsync(string serverName, string resourceUri, CancellationToken cancellationToken = default)
        {
            var parameters = new Dictionary<string, string> { { "uri", resourceUri } };
            var response = await SendRequestAsync(serverName, "resources/read", parameters, cancellationToken);

            if (response.RootElement.TryGetProperty("result", out var resultProp))
            {
                return resultProp.GetRawText();
            }

            throw new InvalidOperationException("Failed to read resource path location content.");
        }

        public Task<IEnumerable<McpPrompt>> ListPromptsAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IEnumerable<McpPrompt>>(Array.Empty<McpPrompt>());
        }

        public Task<McpPrompt> GetPromptAsync(string serverName, string promptName, Dictionary<string, string> arguments, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        private async Task<JsonDocument> SendRequestAsync(string serverName, string method, object? parameters, CancellationToken cancellationToken)
        {
            if (!_serverPool.TryGetValue(serverName, out var streams))
            {
                throw new KeyNotFoundException($"Server '{serverName}' transport channels are unallocated.");
            }

            var id = Interlocked.Increment(ref _requestId).ToString();
            var tcs = new TaskCompletionSource<JsonDocument>(TaskCreationOptions.RunContinuationsAsynchronously);
            _pendingRequests[id] = tcs;

            var payload = new
            {
                jsonrpc = "2.0",
                id = id,
                method = method,
                @params = parameters
            };

            var jsonLine = JsonSerializer.Serialize(payload);
            var writer = new StreamWriter(streams.RequestStream) { AutoFlush = true };

            await writer.WriteLineAsync(jsonLine.AsMemory(), cancellationToken);
            _tracer.LogTrace($"[McpClient] Dispatched JSON-RPC framework packet [{id}] to server '{serverName}': {method}");

            using (cancellationToken.Register(() => _pendingRequests.TryRemove(id, out _)))
            {
                return await tcs.Task;
            }
        }
    }
}