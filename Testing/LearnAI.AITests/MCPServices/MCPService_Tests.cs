using LearnAI.MCP.Interfaces;
using LearnAI.MCP.Models;
using LearnAI.MCP.Services;
using Moq;
using OpenAI.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using McpTool = LearnAI.MCP.Models.McpTool;

namespace LearnAI.AITests.MCPServices
{
    public class MCPService_Tests
    {
        private readonly Mock<IMcpClient> _mockMcpClient;
        private readonly McpTracingService _tracer;
        private readonly McpService _mcpService;

        public MCPService_Tests()
        {
            _mockMcpClient = new Mock<IMcpClient>();
            _tracer = new McpTracingService();
            _mcpService = new McpService(_mockMcpClient.Object, _tracer);
        }

        [Fact]
        public async Task ProcessToolTokenPipelineAsync_ValidPayload_ReturnsExpectedJsonResult()
        {
            // Arrange
            string serverName = "LocalMathServer";
            string toolName = "calculate";
            var arguments = new Dictionary<string, object> { { "a", 10 }, { "b", 5 }, { "op", "multiply" } };
            string expectedJsonResult = "{\"value\":50}";

            _mockMcpClient
                .Setup(c => c.ExecuteToolAsync(serverName, toolName, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedJsonResult);

            // Act
            string result = await _mcpService.ProcessToolTokenPipelineAsync(serverName, toolName, arguments, CancellationToken.None);

            // Assert
            Assert.Equal(expectedJsonResult, result);
            _mockMcpClient.Verify(c => c.ExecuteToolAsync(serverName, toolName, It.Is<string>(json => json.Contains("multiply")), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ProcessToolTokenPipelineAsync_ClientReturnsNull_ThrowsInvalidOperationException()
        {
            // Arrange
            string serverName = "LocalMathServer";
            string toolName = "calculate";
            var arguments = new Dictionary<string, object>();

            _mockMcpClient
                .Setup(c => c.ExecuteToolAsync(serverName, toolName, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string)null!);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _mcpService.ProcessToolTokenPipelineAsync(serverName, toolName, arguments, CancellationToken.None));
        }

        [Fact]
        public async Task ProcessToolTokenPipelineAsync_ClientThrowsException_PropagatesException()
        {
            // Arrange
            string serverName = "LocalMathServer";
            string toolName = "calculate";
            var arguments = new Dictionary<string, object>();

            _mockMcpClient
                .Setup(c => c.ExecuteToolAsync(serverName, toolName, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new TimeoutException("Remote transport connection failed"));

            // Act & Assert
            await Assert.ThrowsAsync<TimeoutException>(() =>
                _mcpService.ProcessToolTokenPipelineAsync(serverName, toolName, arguments, CancellationToken.None));
        }

        [Fact]
        public async Task AggregateAvailableToolsAsync_SuccessfulDiscovery_ReturnsAllTools()
        {
            // Arrange
            var mockToolsList = new List<McpTool>
            {
                new() { Name = "calculate", Description = "Math calculations" },
                new() { Name = "get_weather", Description = "Weather reports" }
            };

            _mockMcpClient
                .Setup(c => c.ListToolsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockToolsList);

            // Act
            var result = await _mcpService.AggregateAvailableToolsAsync(CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, t => t.Name == "calculate");
            Assert.Contains(result, t => t.Name == "get_weather");
        }

        [Fact]
        public async Task AggregateAvailableToolsAsync_ClientFails_SwallowsExceptionAndReturnsEmptyCollection()
        {
            // Arrange
            _mockMcpClient
                .Setup(c => c.ListToolsAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Network failure"));

            // Act
            var result = await _mcpService.AggregateAvailableToolsAsync(CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}