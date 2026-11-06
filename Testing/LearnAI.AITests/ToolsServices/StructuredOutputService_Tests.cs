using System;
using System.Text.Json;
using Xunit;
using LearnAI.Tools.Services;
using LearnAI.Tools.Models;

namespace LearnAI.AITests.ToolsServices
{
    public class StructuredOutputService_Tests
    {
        private readonly StructuredOutputService _service;

        public StructuredOutputService_Tests()
        {
            var schemaBuilder = new JsonSchemaBuilder();
            _service = new StructuredOutputService(schemaBuilder);
        }

        private class MockPayload
        {
            public string Status { get; set; } = string.Empty;
            public int Code { get; set; }
        }

        [Fact]
        public void GenerateSchemaForType_ShouldProduceValidJsonSchema()
        {
            var schemaJson = _service.GenerateSchemaForType<MockPayload>();
            Assert.False(string.IsNullOrWhiteSpace(schemaJson));

            using var doc = JsonDocument.Parse(schemaJson);
            var root = doc.RootElement;

            Assert.Equal("object", root.GetProperty("type").GetString());

            var properties = root.GetProperty("properties");

            Assert.True(properties.TryGetProperty("payloadStructure", out var payloadParam));

            var payloadType = payloadParam.GetProperty("type").GetString();
            Assert.Equal("object", payloadType);
        }


        [Fact]
        public void DeserializeWithRetry_ShouldStripMarkdownCodeBlocksCleanly()
        {
            var rawJsonWithMarkdown = "```json\n{\n  \"Status\": \"Active\",\n  \"Code\": 200\n}\n```";

            var result = _service.DeserializeWithRetry<MockPayload>(rawJsonWithMarkdown);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal("Active", result.Data.Status);
            Assert.Equal(200, result.Data.Code);
            Assert.Equal(1, result.AttemptsTaken);
        }

        [Fact]
        public void DeserializeWithRetry_ShouldInvokeRetryPromptHandlerOnMalformedJson()
        {
            var malformedJson = "{ \"Status\": \"Broken\", \"Code\": "; // Missing value/brackets
            var fixedJson = "{\n  \"Status\": \"Fixed\",\n  \"Code\": 400\n}";
            int handlerCallCount = 0;

            Func<string, string> retryHandler = (errorMessage) =>
            {
                handlerCallCount++;
                return fixedJson;
            };

            var result = _service.DeserializeWithRetry<MockPayload>(malformedJson, retryHandler, maxAttempts: 3);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal("Fixed", result.Data.Status);
            Assert.Equal(400, result.Data.Code);
            Assert.Equal(2, result.AttemptsTaken);
            Assert.Equal(1, handlerCallCount);
        }

        [Fact]
        public void DeserializeWithRetry_ShouldReturnFailureWhenMaxAttemptsExceeded()
        {
            var completelyMalformedJson = "Not even close to JSON string content";

            Func<string, string> brokenRetryHandler = (errorMessage) => completelyMalformedJson;

            var result = _service.DeserializeWithRetry<MockPayload>(completelyMalformedJson, brokenRetryHandler, maxAttempts: 2);

            Assert.False(result.IsSuccess);
            Assert.Null(result.Data);
            Assert.Equal(2, result.AttemptsTaken);
            Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
        }
    }
}