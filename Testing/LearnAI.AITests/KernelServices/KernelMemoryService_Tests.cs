using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Embeddings;
using LearnAI.Kernel.Services;
using CommunityToolkit.VectorData.InMemory;

namespace LearnAI.AITests.KernelServices
{
    public class KernelMemoryService_Tests
    {
        private readonly VectorStore _vectorStore;
        private readonly Mock<ITextEmbeddingGenerationService> _mockEmbeddingService;
        private readonly KernelMemoryService _memoryService;

        public KernelMemoryService_Tests()
        {
            _vectorStore = new InMemoryVectorStore();
            _mockEmbeddingService = new Mock<ITextEmbeddingGenerationService>();
            _memoryService = new KernelMemoryService(_vectorStore, _mockEmbeddingService.Object);
        }

        [Fact]
        public async Task SaveMemoryAsync_ValidInput_PersistsRecordToStore()
        {
            var content = "Semantic Kernel orchestrates AI workflows.";
            var description = "Core concept definition.";
            var mockVector = new float[] { 0.5f };
            var expectedEmbedding = new ReadOnlyMemory<float>(mockVector);

            _mockEmbeddingService
                .Setup(s => s.GenerateEmbeddingsAsync(It.IsAny<IList<string>>(), It.IsAny<Microsoft.SemanticKernel.Kernel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ReadOnlyMemory<float>> { expectedEmbedding });

            var collection = _vectorStore.GetCollection<string, MemoryRecord>("semantic_memory");
            await collection.EnsureCollectionExistsAsync();

            await _memoryService.SaveMemoryAsync(content, description);

            var searchOptions = new VectorSearchOptions<MemoryRecord>();
            var results = collection.SearchAsync(expectedEmbedding, top: 1, searchOptions);

            var items = new List<VectorSearchResult<MemoryRecord>>();
            await foreach (var item in results)
            {
                items.Add(item);
            }

            Assert.Single(items);
            Assert.Equal(content, items[0].Record.Content);
            Assert.Equal(description, items[0].Record.Description);
        }

        [Fact]
        public async Task SearchMemoryAsync_ValidQuery_ReturnsMatchingRecords()
        {
            var query = "workflows";
            var content = "Semantic Kernel orchestrates AI workflows.";
            var description = "Core concept definition.";
            var mockVector = new float[] { 0.5f };
            var expectedEmbedding = new ReadOnlyMemory<float>(mockVector);

            _mockEmbeddingService
                .Setup(s => s.GenerateEmbeddingsAsync(It.IsAny<IList<string>>(), It.IsAny<Microsoft.SemanticKernel.Kernel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ReadOnlyMemory<float>> { expectedEmbedding });

            var collection = _vectorStore.GetCollection<string, MemoryRecord>("semantic_memory");
            await collection.EnsureCollectionExistsAsync();

            await collection.UpsertAsync(new MemoryRecord
            {
                Content = content,
                Description = description,
                Vector = expectedEmbedding
            });

            var searchResults = await _memoryService.SearchMemoryAsync(query, maxResults: 1);

            Assert.Single(searchResults);
            Assert.Equal(content, searchResults[0].Content);
            Assert.Equal(description, searchResults[0].Description);
        }
    }
}