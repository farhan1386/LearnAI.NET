using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LearnAI.RAG.Services;
using Microsoft.Extensions.AI;
using Moq;
using Xunit;

namespace LearnAI.AITests.RAGServices
{
    public class VectorService_Tests
    {
        private readonly Mock<IEmbeddingGenerator<string, Embedding<float>>> _mockGenerator;
        private readonly VectorService _vectorService;

        public VectorService_Tests()
        {
            _mockGenerator = new Mock<IEmbeddingGenerator<string, Embedding<float>>>();
            _vectorService = new VectorService(_mockGenerator.Object);
        }

        [Fact]
        public void Constructor_NullGenerator_ThrowsArgumentNullException()
        {
            // Act   // Assert
            Assert.Throws<ArgumentNullException>(() => new VectorService(null!));
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task GenerateEmbeddingAsync_InvalidText_ReturnsEmptyMemory(string invalidText)
        {
            // Act
            var result = await _vectorService.GenerateEmbeddingAsync(invalidText);

            // Assert
            Assert.True(result.IsEmpty);
        }

        [Fact]
        public async Task GenerateEmbeddingAsync_ValidText_ReturnsExpectedEmbedding()
        {
            // Arrange
            var text = "Sample text";
            var expectedVector = new float[] { 0.1f, 0.2f, 0.3f };
            var embedding = new Embedding<float>(expectedVector);
            var generatedEmbeddings = new GeneratedEmbeddings<Embedding<float>>(new[] { embedding });

            _mockGenerator
                .Setup(g => g.GenerateAsync(
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<EmbeddingGenerationOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(generatedEmbeddings);

            // Act
            var result = await _vectorService.GenerateEmbeddingAsync(text);

            // Assert
            Assert.Equal(expectedVector, result.ToArray());
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GenerateEmbeddingsBatchAsync_EmptyOrNullInput_ReturnsEmptyList(bool isNull)
        {
            // Arrange
            List<string>? inputList = isNull ? null : new List<string>();

            // Act
            var result = await _vectorService.GenerateEmbeddingsBatchAsync(inputList!);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GenerateEmbeddingsBatchAsync_ValidBatch_ReturnsExpectedEmbeddingsList()
        {
            // Arrange
            var texts = new List<string> { "text1", "text2" };
            var vector1 = new float[] { 0.1f, 0.2f };
            var vector2 = new float[] { 0.3f, 0.4f };

            var embeddings = new List<Embedding<float>>
            {
                new Embedding<float>(vector1),
                new Embedding<float>(vector2)
            };
            var generatedEmbeddings = new GeneratedEmbeddings<Embedding<float>>(embeddings);

            _mockGenerator
                .Setup(g => g.GenerateAsync(
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<EmbeddingGenerationOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(generatedEmbeddings);

            // Act
            var result = await _vectorService.GenerateEmbeddingsBatchAsync(texts);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(vector1, result[0].ToArray());
            Assert.Equal(vector2, result[1].ToArray());
        }
    }
}