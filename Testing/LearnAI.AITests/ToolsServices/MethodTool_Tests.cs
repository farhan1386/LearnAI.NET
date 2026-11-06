using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using LearnAI.Tools.Interfaces;
using LearnAI.Tools.Services;

namespace LearnAI.AITests.ToolsServices
{
    public class MethodTool_Tests
    {
        private readonly IJsonSchemaBuilder _mockSchemaBuilder;

        public MethodTool_Tests()
        {
            _mockSchemaBuilder = new JsonSchemaBuilder();
        }

        private class MockTargetService
        {
            public string Add(int a, int b)
            {
                return (a + b).ToString();
            }

            public async Task<string> ProcessAsync(string text)
            {
                await Task.Delay(1);
                return $"Processed: {text}";
            }

            public void Crash()
            {
                throw new InvalidOperationException("Internal service exception occurred.");
            }
        }

        [Fact]
        public async Task ExecuteAsync_ShouldInvokeSynchronousMethodWithCorrectParameters()
        {
            var target = new MockTargetService();
            var methodInfo = typeof(MockTargetService).GetMethod(nameof(MockTargetService.Add));
            Assert.NotNull(methodInfo);

            var tool = new MethodTool(target, methodInfo, _mockSchemaBuilder);
            var argumentsJson = "{\"a\": 30, \"b\": 20}";

            var result = await tool.ExecuteAsync(argumentsJson);

            Assert.Equal("50", result);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldHandleAndUnpackAsynchronousTaskMethods()
        {
            var target = new MockTargetService();
            var methodInfo = typeof(MockTargetService).GetMethod(nameof(MockTargetService.ProcessAsync));
            Assert.NotNull(methodInfo);

            var tool = new MethodTool(target, methodInfo, _mockSchemaBuilder);
            var argumentsJson = "{\"text\": \"hello\"}";

            var result = await tool.ExecuteAsync(argumentsJson);

            Assert.Equal("Processed: hello", result);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldThrowWhenRequiredParameterIsMissing()
        {
            var target = new MockTargetService();
            var methodInfo = typeof(MockTargetService).GetMethod(nameof(MockTargetService.Add));
            Assert.NotNull(methodInfo);

            var tool = new MethodTool(target, methodInfo, _mockSchemaBuilder);
            var invalidArgumentsJson = "{\"a\": 30}";

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => tool.ExecuteAsync(invalidArgumentsJson));
            Assert.Contains("Missing required parameter: 'b'", exception.Message);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldUnpackTargetInvocationExceptionsGracefully()
        {
            var target = new MockTargetService();
            var methodInfo = typeof(MockTargetService).GetMethod(nameof(MockTargetService.Crash));
            Assert.NotNull(methodInfo);

            var tool = new MethodTool(target, methodInfo, _mockSchemaBuilder);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => tool.ExecuteAsync("{}"));
            Assert.Contains("Execution failed inside Crash", exception.Message);
            Assert.Contains("Internal service exception occurred.", exception.Message);
        }
    }
}
