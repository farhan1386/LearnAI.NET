using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Moq;
using Xunit;
using LearnAI.Azure.Services;

namespace LearnAI.AITests.AzureServices
{
    public class ContentService_Tests
    {
        private readonly Mock<IChatClient> _mockInnerClient;

        public ContentService_Tests()
        {
            _mockInnerClient = new Mock<IChatClient>();
        }

        [Fact]
        public async Task GetResponseAsync_WithCleanPrompt_PassesThroughToInnerClient()
        {
            var expectedResponse = new ChatResponse(new List<ChatMessage> { new ChatMessage(ChatRole.Assistant, "Clean response") });

            _mockInnerClient
                .Setup(c => c.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var messages = new List<ChatMessage> { new ChatMessage(ChatRole.User, "This is a clean, perfectly compliant prompt.") };

            // Using 'azure.com' triggers the safe plug-and-play bypass mode to avoid live cloud HTTP network calls
            var contentService = new ContentService(_mockInnerClient.Object, "https://azure.com");

            var actualResponse = await contentService.GetResponseAsync(messages);

            Assert.NotNull(actualResponse);
            Assert.Equal("Clean response", actualResponse.Text);
            _mockInnerClient.Verify(c => c.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetResponseAsync_WithEmptyMessages_DoesNotTriggerAnalysisAndPassesThrough()
        {
            var expectedResponse = new ChatResponse(new List<ChatMessage> { new ChatMessage(ChatRole.Assistant, "Empty input handled") });

            _mockInnerClient
                .Setup(c => c.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var messages = new List<ChatMessage>();
            var contentService = new ContentService(_mockInnerClient.Object, "https://azure.com");

            var actualResponse = await contentService.GetResponseAsync(messages);

            Assert.NotNull(actualResponse);
            _mockInnerClient.Verify(c => c.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}