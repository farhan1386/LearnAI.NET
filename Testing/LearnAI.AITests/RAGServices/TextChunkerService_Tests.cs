using System;
using System.Collections.Generic;
using LearnAI.RAG.Models;
using LearnAI.RAG.Services;
using Xunit;

namespace LearnAI.AITests.RAGServices
{
    public class TextChunkerService_Tests
    {
        private readonly TextChunkerService _chunkerService;

        public TextChunkerService_Tests()
        {
            _chunkerService = new TextChunkerService();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void SplitIntoChunks_InvalidText_ReturnsEmptyList(string invalidText)
        {
            // Act
            var result = _chunkerService.SplitIntoChunks(invalidText);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void SplitIntoChunks_TextShorterThanChunkSize_ReturnsSingleChunk()
        {
            // Arrange
            var text = "Hello World";
            int chunkSize = 50;
            int overlap = 5;

            // Act
            var result = _chunkerService.SplitIntoChunks(text, chunkSize, overlap);

            // Assert
            Assert.Single(result);
            Assert.Equal(text, result[0].Content);
            Assert.Equal(0, result[0].StartCharacterIndex);
            Assert.Equal(text.Length, result[0].EndCharacterIndex);
            Assert.Equal(0, result[0].ChunkIndex);
        }

        [Fact]
        public void SplitIntoChunks_TextTriggersMultipleChunksWithOverlap_CalculatesIndicesCorrectly()
        {
            // Arrange
            var text = "abcdefghij"; // 10 characters
            int chunkSize = 4;
            int overlap = 2;

            // Act
            var result = _chunkerService.SplitIntoChunks(text, chunkSize, overlap);

            // Assert
            Assert.Equal(4, result.Count);

            // Chunk 0: "abcd" (Start: 0, End: 4)
            Assert.Equal("abcd", result[0].Content);
            Assert.Equal(0, result[0].StartCharacterIndex);
            Assert.Equal(4, result[0].EndCharacterIndex);
            Assert.Equal(0, result[0].ChunkIndex);

            // Chunk 1: "cdef" (Start: 2, End: 6)
            Assert.Equal("cdef", result[1].Content);
            Assert.Equal(2, result[1].StartCharacterIndex);
            Assert.Equal(6, result[1].EndCharacterIndex);
            Assert.Equal(1, result[1].ChunkIndex);

            // Chunk 2: "efgh" (Start: 4, End: 8)
            Assert.Equal("efgh", result[2].Content);
            Assert.Equal(4, result[2].StartCharacterIndex);
            Assert.Equal(8, result[2].EndCharacterIndex);
            Assert.Equal(2, result[2].ChunkIndex);

            // Chunk 3: "ghij" (Start: 6, End: 10)
            Assert.Equal("ghij", result[3].Content);
            Assert.Equal(6, result[3].StartCharacterIndex);
            Assert.Equal(10, result[3].EndCharacterIndex);
            Assert.Equal(3, result[3].ChunkIndex);
        }
    }
}