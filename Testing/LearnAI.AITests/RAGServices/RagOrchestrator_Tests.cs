using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LearnAI.RAG.Interfaces;
using LearnAI.RAG.Models;
using LearnAI.RAG.Services;
using Microsoft.Extensions.AI;
using Moq;
using Xunit;

namespace LearnAI.AITests.RAGServices
{
    public class RagOrchestrator_Tests
    {
        private readonly Mock<IEmbeddingService> _mockEmbeddingService;
        private readonly Mock<IVectorDatabase> _mockVectorDb;
        private readonly Mock<IChatClient> _mockChatClient;
        private readonly TextChunkerService _chunkerService;
        private readonly RagOrchestrator _orchestrator;

        public RagOrchestrator_Tests()
        {
            _mockEmbeddingService = new Mock<IEmbeddingService>();
            _mockVectorDb = new Mock<IVectorDatabase>();
            _mockChatClient = new Mock<IChatClient>();
            _chunkerService = new TextChunkerService();

            _orchestrator = new RagOrchestrator(
                _chunkerService,
                _mockEmbeddingService.Object,
                _mockVectorDb.Object,
                _mockChatClient.Object
            );
        }

        [Fact]
        public async Task IngestDocumentAsync_ValidDocument_ProcessesAndSavesChunks()
        {
            // Arrange
            var content = "This is a single sample test document content.";
            var docName = "test.txt";
            var mockVector = new ReadOnlyMemory<float>(new float[] { 0.1f, 0.2f });

            _mockEmbeddingService
                .Setup(s => s.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockVector);

            _mockVectorDb
                .Setup(db => db.SaveDocumentAsync(It.IsAny<VectorDocument>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _orchestrator.IngestDocumentAsync(content, docName);

            // Assert
            _mockEmbeddingService.Verify(s => s.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            _mockVectorDb.Verify(db => db.SaveDocumentAsync(It.Is<VectorDocument>(d => d.SourceDocumentName == docName), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task QueryAsync_ValidPrompt_ReturnsExpectedResponse()
        {
            // Arrange
            var userPrompt = "What is the result?";
            var expectedResponseText = "The final result calculation.";
            var mockVector = new ReadOnlyMemory<float>(new float[] { 0.3f, 0.4f });

            var mockSearchResult = new List<VectorQueryResult>
            {
                new VectorQueryResult
                {
                    Document = new VectorDocument { Content = "Context document data string." },
                    SimilarityScore = 0.95f
                }
            };

            var chatResponse = new Microsoft.Extensions.AI.ChatResponse(
                new Microsoft.Extensions.AI.ChatMessage(Microsoft.Extensions.AI.ChatRole.Assistant, expectedResponseText)
            );

            _mockEmbeddingService
                .Setup(s => s.GenerateEmbeddingAsync(userPrompt, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockVector);

            _mockVectorDb
                .Setup(db => db.SearchSimilarAsync(mockVector, 3, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockSearchResult);

            _mockChatClient
                .Setup(c => c.GetResponseAsync(
                    It.IsAny<IEnumerable<Microsoft.Extensions.AI.ChatMessage>>(),
                    It.IsAny<Microsoft.Extensions.AI.ChatOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(chatResponse);

            // Act
            var result = await _orchestrator.QueryAsync(userPrompt);

            // Assert
            Assert.Equal(expectedResponseText, result);
            _mockEmbeddingService.Verify(s => s.GenerateEmbeddingAsync(userPrompt, It.IsAny<CancellationToken>()), Times.Once);
            _mockVectorDb.Verify(db => db.SearchSimilarAsync(mockVector, 3, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}