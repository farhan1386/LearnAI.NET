using System;
using System.Collections.Generic;
using Xunit;
using LearnAI.Tools.Interfaces;
using LearnAI.Tools.Models;
using LearnAI.Tools.Services;

namespace LearnAI.AITests.ToolsServices
{
    public class ToolExecutionEngine_Tests
    {
        private class MockOptions
        {
            public object? ToolChoice { get; set; }
        }

        private class MockTool : IAITool
        {
            public ToolDefinition Definition { get; } = new()
            {
                Name = "TestTool",
                Description = "A mock tool for testing execution pipelines.",
                ParametersSchemaJson = "{}"
            };

            public System.Threading.Tasks.Task<string> ExecuteAsync(string argumentsJson)
            {
                return System.Threading.Tasks.Task.FromResult("SuccessResult");
            }
        }

        private class MockMiddleware : IToolMiddleware
        {
            public bool WasInvoked { get; private set; }

            public async System.Threading.Tasks.Task<string> InvokeAsync(ToolExecutionContext context, Func<System.Threading.Tasks.Task<string>> next)
            {
                WasInvoked = true;
                context.Metadata["MiddlewareKey"] = "PassedThrough";
                return await next();
            }
        }

        [Fact]
        public void ApplySelectionPolicy_ShouldMapNoneStrategyCorrectly()
        {
            var engine = new ToolExecutionEngine(new List<IToolMiddleware>());
            var options = new MockOptions();

            engine.ApplySelectionPolicy(options, ToolSelectionMode.None);

            Assert.Equal("none", options.ToolChoice);
        }

        [Fact]
        public void ApplySelectionPolicy_ShouldMapAutoStrategyCorrectly()
        {
            var engine = new ToolExecutionEngine(new List<IToolMiddleware>());
            var options = new MockOptions();

            engine.ApplySelectionPolicy(options, ToolSelectionMode.Auto);

            Assert.Equal("auto", options.ToolChoice);
        }

        [Fact]
        public void ApplySelectionPolicy_ShouldThrowWhenRequiredToolIsNotRegistered()
        {
            var engine = new ToolExecutionEngine(new List<IToolMiddleware>());
            var options = new MockOptions();

            Assert.Throws<InvalidOperationException>(() =>
                engine.ApplySelectionPolicy(options, ToolSelectionMode.Required, "NonExistentTool")
            );
        }

        [Fact]
        public void ApplySelectionPolicy_ShouldMapRequiredStrategyCorrectlyWhenRegistered()
        {
            var engine = new ToolExecutionEngine(new List<IToolMiddleware>());
            var tool = new MockTool();
            engine.RegisterTool(tool);
            var options = new MockOptions();

            engine.ApplySelectionPolicy(options, ToolSelectionMode.Required, "TestTool");

            Assert.NotNull(options.ToolChoice);
            var choiceType = options.ToolChoice.GetType();
            var typeProp = choiceType.GetProperty("type");

            Assert.NotNull(typeProp);
            Assert.Equal("function", typeProp.GetValue(options.ToolChoice));
        }

        [Fact]
        public async System.Threading.Tasks.Task ExecuteToolWithMiddlewareAsync_ShouldCascadeThroughInterceptors()
        {
            var middleware = new MockMiddleware();
            var engine = new ToolExecutionEngine(new[] { middleware });
            var tool = new MockTool();
            engine.RegisterTool(tool);

            var result = await engine.ExecuteToolWithMiddlewareAsync("TestTool", "{}");

            Assert.Equal("SuccessResult", result);
            Assert.True(middleware.WasInvoked);
        }

        [Fact]
        public async System.Threading.Tasks.Task ExecuteToolWithMiddlewareAsync_ShouldThrowWhenToolNotFound()
        {
            var engine = new ToolExecutionEngine(new List<IToolMiddleware>());

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                engine.ExecuteToolWithMiddlewareAsync("UnregisteredTool", "{}")
            );
        }
    }
}