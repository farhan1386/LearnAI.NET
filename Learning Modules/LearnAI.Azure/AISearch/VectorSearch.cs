using Azure.Identity;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;

namespace LearnAI.Azure.AISearch
{
    public class VectorSearch
    {
        private readonly SearchClient _searchClient;

        public VectorSearch(string endpoint, string indexName)
        {
            _searchClient = new SearchClient(new Uri(endpoint), indexName, new DefaultAzureCredential());
        }

        public async Task<List<SearchDocument>> HybridSearchAsync(string text, ReadOnlyMemory<float> vectors, int topK = 5)
        {
            var options = new SearchOptions { Size = topK };
            options.VectorSearch = new VectorSearchOptions
            {
                Queries = { new VectorizedQuery(vectors) { KNearestNeighborsCount = topK, Fields = { "contentVector" } } }
            };

            var results = new List<SearchDocument>();
            var searchResult = await _searchClient.SearchAsync<SearchDocument>(text, options);

            await foreach (var item in searchResult.Value.GetResultsAsync())
            {
                results.Add(item.Document);
            }
            return results;
        }
    }
}