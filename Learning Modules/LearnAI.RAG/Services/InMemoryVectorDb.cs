using LearnAI.RAG.Interfaces;
using LearnAI.RAG.Models;
using System.Collections.Concurrent;

namespace LearnAI.RAG.Services
{
    public class InMemoryVectorDb : IVectorDatabase
    {
        private readonly ConcurrentBag<VectorDocument> _store = new();

        public Task SaveDocumentAsync(VectorDocument document, CancellationToken cancellationToken = default)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            _store.Add(document);
            return Task.CompletedTask;
        }

        public Task<List<VectorQueryResult>> SearchSimilarAsync(ReadOnlyMemory<float> queryEmbedding, int limit = 5, CancellationToken cancellationToken = default)
        {
            var results = new List<VectorQueryResult>();
            var querySpan = queryEmbedding.Span;

            foreach (var doc in _store)
            {
                var docSpan = doc.Embedding.Span;
                if (querySpan.Length != docSpan.Length) continue;

                float dotProduct = 0;
                float queryNorm = 0;
                float docNorm = 0;

                for (int i = 0; i < querySpan.Length; i++)
                {
                    dotProduct += querySpan[i] * docSpan[i];
                    queryNorm += querySpan[i] * querySpan[i];
                    docNorm += docSpan[i] * docSpan[i];
                }

                float similarity = (queryNorm == 0 || docNorm == 0)
                    ? 0
                    : dotProduct / (MathF.Sqrt(queryNorm) * MathF.Sqrt(docNorm));

                results.Add(new VectorQueryResult
                {
                    Document = doc,
                    SimilarityScore = similarity
                });
            }

            var topMatches = results
                .OrderByDescending(r => r.SimilarityScore)
                .Take(limit)
                .ToList();

            return Task.FromResult(topMatches);
        }
    }
}