using System;
using System.IO;
using LearnAI.MCP.Services;
using Xunit;

namespace LearnAI.AITests.MCPServices
{
    public class McpTracingService_Tests : IDisposable
    {
        private readonly TextWriter _originalOutput;
        private readonly StringWriter _testOutput;
        private readonly McpTracingService _tracingService;

        public McpTracingService_Tests()
        {
            _originalOutput = Console.Out;
            _testOutput = new StringWriter();
            Console.SetOut(_testOutput);
            _tracingService = new McpTracingService();
        }

        [Fact]
        public void LogTrace_ValidMessage_WritesTraceFormattedStringToConsole()
        {
            string message = "Initializing test duplex memory stream.";

            _tracingService.LogTrace(message);

            string output = _testOutput.ToString();
            Assert.Contains("[TRACE]", output);
            Assert.Contains(message, output);
        }

        [Fact]
        public void LogError_ValidMessage_WritesErrorFormattedStringToConsole()
        {
            string message = "Protocol exception encountered on stream closure.";

            _tracingService.LogError(message);

            string output = _testOutput.ToString();
            Assert.Contains("[ERROR]", output);
            Assert.Contains(message, output);
        }

        public void Dispose()
        {
            Console.SetOut(_originalOutput);
            _testOutput.Dispose();
        }
    }
}