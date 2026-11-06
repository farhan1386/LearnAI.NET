using LearnAI.MCP.Interfaces;
using LearnAI.MCP.Models;
using LearnAI.MCP.Prompts;
using LearnAI.MCP.Resources;
using LearnAI.MCP.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LearnAI.MCP.Server
{
    public class McpServer : IMcpServer
    {
        private readonly IToolRegistry _toolRegistry;
        private readonly IResourceRegistry _resourceRegistry;
        private readonly McpTracingService _tracer;
        private CancellationTokenSource? _cts;

        public McpServerInfo ServerInfo { get; }

        public McpServer(
            McpServerInfo serverInfo,
            IToolRegistry toolRegistry,
            IResourceRegistry resourceRegistry,
            McpTracingService tracer)
        {
            ServerInfo = serverInfo ?? throw new ArgumentNullException(nameof(serverInfo));
            _toolRegistry = toolRegistry ?? throw new ArgumentNullException(nameof(toolRegistry));
            _resourceRegistry = resourceRegistry ?? throw new ArgumentNullException(nameof(resourceRegistry));
            _tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));
        }

        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _tracer.LogTrace($"[McpServer] {ServerInfo.Name} started running version {ServerInfo.Version}.");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            _cts?.Cancel();
            _tracer.LogTrace($"[McpServer] {ServerInfo.Name} shut down gracefully.");
            return Task.CompletedTask;
        }

        public async Task ProcessRequestStreamAsync(Stream inputStream, Stream outputStream, CancellationToken cancellationToken = default)
        {
            using var reader = new StreamReader(inputStream);
            using var writer = new StreamWriter(outputStream) { AutoFlush = true };

            while (!cancellationToken.IsCancellationRequested && (_cts == null || !_cts.IsCancellationRequested))
            {
                var line = await reader.ReadLineAsync(cancellationToken);
                if (string.IsNullOrEmpty(line)) break;

                try
                {
                    using var doc = JsonDocument.Parse(line);
                    var root = doc.RootElement;

                    if (!root.TryGetProperty("method", out var methodProp)) continue;
                    var method = methodProp.GetString();
                    var id = root.TryGetProperty("id", out var idProp) ? idProp.Clone() : default;

                    string responseJson = method switch
                    {
                        "tools/list" => await HandleListToolsResponseAsync(id, cancellationToken),
                        "tools/call" => await HandleCallToolResponseAsync(id, root, cancellationToken),
                        "resources/list" => await HandleListResourcesResponseAsync(id, cancellationToken),
                        _ => CreateErrorResponse(id, -32601, "Method not found")
                    };

                    await writer.WriteLineAsync(responseJson.AsMemory(), cancellationToken);
                }
                catch (Exception ex)
                {
                    _tracer.LogError($"[McpServer] Protocol processing error: {ex.Message}");
                }
            }
        }

        public async Task<IEnumerable<McpTool>> HandleListToolsAsync(CancellationToken cancellationToken = default)
        {
            return await _toolRegistry.GetToolsAsync(cancellationToken);
        }

        public async Task<string> HandleCallToolAsync(string name, string argumentsJson, CancellationToken cancellationToken = default)
        {
            return await _toolRegistry.ExecuteToolAsync(name, argumentsJson, cancellationToken);
        }

        public async Task<IEnumerable<McpResource>> HandleListResourcesAsync(CancellationToken cancellationToken = default)
        {
            return await _resourceRegistry.GetResourcesAsync(cancellationToken);
        }

        public async Task<string> HandleReadResourceAsync(string uri, CancellationToken cancellationToken = default)
        {
            return await _resourceRegistry.ReadResourceAsync(uri, cancellationToken);
        }

        public Task<IEnumerable<McpPrompt>> HandleListPromptsAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IEnumerable<McpPrompt>>(Array.Empty<McpPrompt>());
        }

        public Task<McpPrompt> HandleGetPromptAsync(string name, Dictionary<string, string> arguments, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        private async Task<string> HandleListToolsResponseAsync(JsonElement id, CancellationToken cancellationToken)
        {
            var tools = await HandleListToolsAsync(cancellationToken);
            return JsonSerializer.Serialize(new { jsonrpc = "2.0", result = new { tools }, id });
        }

        private async Task<string> HandleCallToolResponseAsync(JsonElement id, JsonElement root, CancellationToken cancellationToken)
        {
            if (!root.TryGetProperty("params", out var paramsProp) || !paramsProp.TryGetProperty("name", out var nameProp))
            {
                return CreateErrorResponse(id, -32602, "Invalid parameters");
            }

            var toolName = nameProp.GetString() ?? string.Empty;
            var argsJson = paramsProp.TryGetProperty("arguments", out var argsProp) ? argsProp.GetRawText() : "{}";

            var result = await HandleCallToolAsync(toolName, argsJson, cancellationToken);
            return JsonSerializer.Serialize(new { jsonrpc = "2.0", result, id });
        }

        private async Task<string> HandleListResourcesResponseAsync(JsonElement id, CancellationToken cancellationToken)
        {
            var resources = await HandleListResourcesAsync(cancellationToken);
            return JsonSerializer.Serialize(new { jsonrpc = "2.0", result = new { resources }, id });
        }

        private string CreateErrorResponse(JsonElement id, int code, string message)
        {
            return JsonSerializer.Serialize(new { jsonrpc = "2.0", error = new { code, message }, id });
        }
    }
}