using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LearnAI.RAG.Models;
using LearnAI.RAG.Services;
using Xunit;

namespace LearnAI.AITests.RAGServices
{
    public class InMemoryVectorDb_Tests
    {
        private readonly InMemoryVectorDb _vectorDb;

        public InMemoryVectorDb_Tests()
        {
            _vectorDb = new InMemoryVectorDb();
        }

        [Fact]
        public async Task SaveDocumentAsync_NullDocument_ThrowsArgumentNullException()
        {
            // Act   // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _vectorDb.SaveDocumentAsync(null!));
        }

        [Fact]
        public async Task SearchSimilarAsync_MismatchedVectorLengths_SkipsDocuments()
        {
            // Arrange
            var doc = new VectorDocument
            {
                Content = "Length 2 document",
                Embedding = new float[] { 1.0f, 0.0f }
            };
            await _vectorDb.SaveDocumentAsync(doc);

            var queryVector = new ReadOnlyMemory<float>(new float[] { 1.0f, 0.0f, 0.0f });

            // Act
            var results = await _vectorDb.SearchSimilarAsync(queryVector);

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public async Task SearchSimilarAsync_ValidQueries_ReturnsCorrectSimilarityAndLimitsResults()
        {
            // Arrange
            var doc1 = new VectorDocument { Content = "Doc 1", Embedding = new float[] { 1.0f, 0.0f } };
            var doc2 = new VectorDocument { Content = "Doc 2", Embedding = new float[] { 0.0f, 1.0f } };
            var doc3 = new VectorDocument { Content = "Doc 3", Embedding = new float[] { 1.0f, 1.0f } };

            await _vectorDb.SaveDocumentAsync(doc1);
            await _vectorDb.SaveDocumentAsync(doc2);
            await _vectorDb.SaveDocumentAsync(doc3);

            var queryVector = new ReadOnlyMemory<float>(new float[] { 1.0f, 0.0f });

            // Act
            var results = await _vectorDb.SearchSimilarAsync(queryVector, limit: 2);

            // Assert
            Assert.Equal(2, results.Count);

            Assert.Equal("Doc 1", results[0].Document.Content);
            Assert.Equal(1.0f, results[0].SimilarityScore, precision: 3);

            Assert.Equal("Doc 3", results[1].Document.Content);
            Assert.True(results[1].SimilarityScore > 0.70f);
        }
    }
}