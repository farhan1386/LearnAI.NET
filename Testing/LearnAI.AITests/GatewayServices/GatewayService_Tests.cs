using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using LearnAI.Gateway.Enums;
using LearnAI.Gateway.Factories;
using LearnAI.Gateway.Interfaces;
using LearnAI.Gateway.Models;
using LearnAI.Gateway.Services;
using Microsoft.Extensions.AI;
using Moq;
using Xunit;

namespace LearnAI.AITests.GatewayServices
{
    public class GatewayService_Tests
    {
        private readonly Mock<IChatClient> _mockChatClient;
        private readonly GatewayService _gatewayService;
        private readonly int _expectedProviderCount;

        public GatewayService_Tests()
        {
            _mockChatClient = new Mock<IChatClient>();
            var providerList = new List<IAIProvider>();
            var providerTypes = Enum.GetValues<ProviderType>();
            _expectedProviderCount = providerTypes.Length;

            foreach (ProviderType type in providerTypes)
            {
                var mockProvider = new Mock<IAIProvider>();
                mockProvider.Setup(p => p.CreateClient(It.IsAny<string>())).Returns(_mockChatClient.Object);

                foreach (var prop in typeof(IAIProvider).GetProperties())
                {
                    if (prop.PropertyType == typeof(ProviderType) && prop.CanRead)
                    {
                        var param = Expression.Parameter(typeof(IAIProvider), "p");
                        var body = Expression.Property(param, prop);
                        var lambda = Expression.Lambda<Func<IAIProvider, ProviderType>>(body, param);
                        mockProvider.Setup(lambda).Returns(type);
                    }
                }
                providerList.Add(mockProvider.Object);
            }

            var factory = new AIProviderFactory(providerList);
            _gatewayService = new GatewayService(factory);
        }

        [Fact]
        public async Task ExecutePromptAsync_ValidRequest_ReturnsExpectedText()
        {
            var request = new GatewayRequest
            {
                Provider = ProviderType.OpenAI,
                ModelName = "gpt-4o",
                Prompt = "Hello AI"
            };

            var expectedText = "Hello human!";
            var chatResponse = new ChatResponse(
                new ChatMessage(ChatRole.Assistant, expectedText)
            );

            _mockChatClient
                .Setup(c => c.GetResponseAsync(
                    It.IsAny<IEnumerable<ChatMessage>>(),
                    It.IsAny<ChatOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(chatResponse);

            var result = await _gatewayService.ExecutePromptAsync(request);

            Assert.Equal(expectedText, result);
        }

        [Fact]
        public async Task CompareProvidersAsync_AllProvidersSucceed_ReturnsSuccessfulMetrics()
        {
            var prompt = "Benchmark test";
            var modelName = "test-model";

            var chatResponse = new ChatResponse(
                new ChatMessage(ChatRole.Assistant, "Response")
            )
            {
                Usage = new UsageDetails { TotalTokenCount = 50 }
            };

            _mockChatClient
                .Setup(c => c.GetResponseAsync(
                    It.IsAny<IEnumerable<ChatMessage>>(),
                    It.IsAny<ChatOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(chatResponse);

            var metrics = await _gatewayService.CompareProvidersAsync(prompt, modelName);

            Assert.Equal(_expectedProviderCount, metrics.Count);

            foreach (var metric in metrics)
            {
                Assert.Equal(modelName, metric.ModelUsed);
                Assert.True(metric.IsSuccess);
                Assert.Equal(50, metric.TokenCount);
                Assert.True(metric.Latency >= TimeSpan.Zero);
                Assert.True(string.IsNullOrEmpty(metric.ErrorMessage));
            }
        }

        [Fact]
        public async Task CompareProvidersAsync_ProviderThrowsException_ReturnsFailureMetric()
        {
            var prompt = "Benchmark test";
            var errorMessage = "Connection failed";

            _mockChatClient
                .Setup(c => c.GetResponseAsync(
                    It.IsAny<IEnumerable<ChatMessage>>(),
                    It.IsAny<ChatOptions>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(errorMessage));

            var metrics = await _gatewayService.CompareProvidersAsync(prompt, null);

            Assert.Equal(_expectedProviderCount, metrics.Count);
            foreach (var metric in metrics)
            {
                Assert.Equal("Default", metric.ModelUsed);
                Assert.False(metric.IsSuccess);
                Assert.Equal(errorMessage, metric.ErrorMessage);
                Assert.Equal(0, metric.TokenCount);
                Assert.True(metric.Latency >= TimeSpan.Zero);
            }
        }
    }
}