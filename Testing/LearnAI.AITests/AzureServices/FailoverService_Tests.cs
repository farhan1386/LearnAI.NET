using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using LearnAI.Azure.Services;

namespace LearnAI.AITests.AzureServices
{
    public class FailoverService_Tests
    {
        private readonly Mock<IChatClient> _mockPrimaryClient;
        private readonly Mock<IChatClient> _mockFallbackClient;
        private readonly Mock<ILogger<FailoverService>> _mockLogger;
        private readonly List<ChatMessage> _testMessages;

        public FailoverService_Tests()
        {
            _mockPrimaryClient = new Mock<IChatClient>();
            _mockFallbackClient = new Mock<IChatClient>();
            _mockLogger = new Mock<ILogger<FailoverService>>();
            _testMessages = new List<ChatMessage> { new ChatMessage(ChatRole.User, "Execute transaction profile") };
        }

        [Fact]
        public async Task GetResponseAsync_PrimarySucceeds_DoesNotInvokeFallbackClient()
        {
            var expectedResponse = new ChatResponse(new List<ChatMessage> { new ChatMessage(ChatRole.Assistant, "Primary Service Success") });

            _mockPrimaryClient
                .Setup(c => c.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var failoverService = new FailoverService(_mockPrimaryClient.Object, _mockFallbackClient.Object, _mockLogger.Object);

            var response = await failoverService.GetResponseAsync(_testMessages);

            Assert.NotNull(response);
            Assert.Equal("Primary Service Success", response.Text);
            _mockPrimaryClient.Verify(c => c.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockFallbackClient.Verify(c => c.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetResponseAsync_PrimaryFails_RoutesExecutionToFallbackClient()
        {
            var fallbackResponse = new ChatResponse(new List<ChatMessage> { new ChatMessage(ChatRole.Assistant, "Secondary Fallback Success") });

            _mockPrimaryClient
                .Setup(c => c.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Primary cluster outage down"));

            _mockFallbackClient
                .Setup(c => c.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(fallbackResponse);

            var failoverService = new FailoverService(_mockPrimaryClient.Object, _mockFallbackClient.Object, _mockLogger.Object);

            var response = await failoverService.GetResponseAsync(_testMessages);

            Assert.NotNull(response);
            Assert.Equal("Secondary Fallback Success", response.Text);
            _mockPrimaryClient.Verify(c => c.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockFallbackClient.Verify(c => c.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetResponseAsync_BothClientsFail_ThrowsExceptionUpstream()
        {
            _mockPrimaryClient
                .Setup(c => c.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Primary down"));

            _mockFallbackClient
                .Setup(c => c.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Secondary down"));

            var failoverService = new FailoverService(_mockPrimaryClient.Object, _mockFallbackClient.Object, _mockLogger.Object);

            await Assert.ThrowsAsync<Exception>(() => failoverService.GetResponseAsync(_testMessages));

            _mockPrimaryClient.Verify(c => c.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockFallbackClient.Verify(c => c.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}