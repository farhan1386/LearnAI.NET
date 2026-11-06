using LearnAI.Chat.Interfaces;
using LearnAI.Chat.Models;
using LearnAI.Chat.Services;
using LearnAI.Tools.Interfaces;
using Microsoft.Extensions.AI;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace LearnAI.AITests.ChatServices
{
    public class ChatService_Test
    {
        private readonly Mock<IChatClientFactory> _mockClientFactory;
        private readonly ConversationMemoryService _memory;
        private readonly Mock<IChatClient> _mockChatClient;
        private readonly Mock<IToolExecutionEngine> _mockToolEngine;
        private readonly ChatService _chatService;

        public ChatService_Test()
        {
            _mockClientFactory = new Mock<IChatClientFactory>();
            _mockChatClient = new Mock<IChatClient>();
            _mockToolEngine = new Mock<IToolExecutionEngine>();
            _memory = new ConversationMemoryService();

            _mockClientFactory
                .Setup(factory => factory.CreateClient(It.IsAny<string>()))
                .Returns(_mockChatClient.Object);

            _chatService = new ChatService(
                _mockClientFactory.Object,
                _memory
            );
        }

        [Fact]
        public async Task AskAsync_ValidRequest_ReturnsSuccessfulChatResponse()
        {
            var request = new ChatRequest { Message = "What is DI?", AIModel = "phi3" };
            var expectedText = "Dependency Injection is a design pattern...";
            var fakeAiResponse = new Microsoft.Extensions.AI.ChatResponse(new ChatMessage(ChatRole.Assistant, expectedText));

            _mockChatClient
                .Setup(client => client.GetResponseAsync(
                    It.IsAny<IList<ChatMessage>>(),
                    It.IsAny<ChatOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(fakeAiResponse);

            var result = await _chatService.AskAsync(request);

            Assert.True(result.IsSuccess);
            Assert.Equal(expectedText, result.ResponseText);
            Assert.Null(result.ErrorMessage);
            Assert.True(result.ExecutionTime > TimeSpan.Zero);
            _mockClientFactory.Verify(factory => factory.CreateClient("phi3"), Times.Once);
        }

        [Fact]
        public async Task AskAsync_EmptyMessage_ReturnsFailureResponse()
        {
            var request = new ChatRequest { Message = "", AIModel = "phi3" };

            var result = await _chatService.AskAsync(request);

            Assert.False(result.IsSuccess);
            Assert.Contains("cannot be null or empty", result.ErrorMessage);
            Assert.Null(result.ResponseText);
            _mockChatClient.Verify(client => client.GetResponseAsync(
                It.IsAny<IList<ChatMessage>>(),
                It.IsAny<ChatOptions>(),
                It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task AskAsync_ClientThrowsException_ReturnsGracefulFailureResponse()
        {
            var request = new ChatRequest { Message = "Break the client", AIModel = "llama3.2" };

            _mockChatClient
                .Setup(client => client.GetResponseAsync(
                    It.IsAny<IList<ChatMessage>>(),
                    It.IsAny<ChatOptions>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Ollama service unavailable"));

            var result = await _chatService.AskAsync(request);

            Assert.False(result.IsSuccess);
            Assert.Contains("Failed to execute AI request: Ollama service unavailable", result.ErrorMessage);
            Assert.Null(result.ResponseText);
            _mockClientFactory.Verify(factory => factory.CreateClient("llama3.2"), Times.Once);
        }
    }
}