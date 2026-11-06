using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Embeddings;

namespace LearnAI.Kernel.Services
{
    public class MemoryRecord
    {
        [VectorStoreKey]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [VectorStoreData]
        public string Content { get; set; } = string.Empty;

        [VectorStoreData]
        public string Description { get; set; } = string.Empty;

        [VectorStoreVector(dimensions: 1536)]
        public ReadOnlyMemory<float> Vector { get; set; }
    }

    public class KernelMemoryService
    {
        private readonly VectorStoreCollection<string, MemoryRecord> _collection;
        private readonly ITextEmbeddingGenerationService _embeddingService;

        public KernelMemoryService(VectorStore vectorStore, ITextEmbeddingGenerationService embeddingService)
        {
            _embeddingService = embeddingService;
            _collection = vectorStore.GetCollection<string, MemoryRecord>("semantic_memory");
        }

        public async Task SaveMemoryAsync(string content, string description)
        {
            var embedding = await _embeddingService.GenerateEmbeddingAsync(content);

            var record = new MemoryRecord
            {
                Content = content,
                Description = description,
                Vector = embedding
            };

            await _collection.UpsertAsync(record);
        }

        public async Task<List<MemoryRecord>> SearchMemoryAsync(string query, int maxResults = 3)
        {
            var results = new List<MemoryRecord>();
            var queryVector = await _embeddingService.GenerateEmbeddingAsync(query);

            var searchOptions = new VectorSearchOptions<MemoryRecord>();

            var searchResult = _collection.SearchAsync(queryVector, top: maxResults, searchOptions);

            await foreach (var item in searchResult)
            {
                results.Add(item.Record);
            }

            return results;
        }
    }
}