using LearnAI.MCP.Interfaces;
using LearnAI.MCP.Models;
using LearnAI.MCP.Tools;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LearnAI.MCP.Services
{
    public class McpService
    {
        private readonly IMcpClient _mcpClient;
        private readonly McpTracingService _tracer;

        public McpService(IMcpClient mcpClient, McpTracingService tracer)
        {
            _mcpClient = mcpClient ?? throw new ArgumentNullException(nameof(mcpClient));
            _tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));
        }

        public async Task<string> ProcessToolTokenPipelineAsync(
            string serverName,
            string toolName,
            Dictionary<string, object> arguments,
            CancellationToken cancellationToken = default)
        {
            _tracer.LogTrace($"[Orchestrator] Initiating tool execution pipeline for tool: '{toolName}' on server: '{serverName}'.");

            try
            {
                string argumentsJson = JsonSerializer.Serialize(arguments);

                _tracer.LogTrace($"[Orchestrator] Forwarding payload string to transport: {argumentsJson}");

                var executionResult = await _mcpClient.ExecuteToolAsync(
                    serverName,
                    toolName,
                    argumentsJson,
                    cancellationToken
                );

                if (executionResult == null)
                {
                    _tracer.LogError($"[Orchestrator] Null execution response received from server '{serverName}' for tool '{toolName}'.");
                    throw new InvalidOperationException("Protocol client returned an empty or invalid result packet.");
                }

                _tracer.LogTrace($"[Orchestrator] Tool pipeline completed successfully for '{toolName}'.");
                return executionResult;
            }
            catch (Exception ex)
            {
                _tracer.LogError($"[Orchestrator] Failed executing tool token pipeline: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<McpTool>> AggregateAvailableToolsAsync(CancellationToken cancellationToken = default)
        {
            _tracer.LogTrace("[Orchestrator] Synchronizing cross-server tool registries.");

            var aggregatedTools = new List<McpTool>();

            try
            {
                var tools = await _mcpClient.ListToolsAsync(cancellationToken);
                aggregatedTools.AddRange(tools);

                _tracer.LogTrace($"[Orchestrator] Aggregated a total of {aggregatedTools.Count} tools for system initialization.");
            }
            catch (Exception ex)
            {
                _tracer.LogError($"[Orchestrator] Error during tool catalog aggregation: {ex.Message}");
            }

            return aggregatedTools;
        }
    }
}