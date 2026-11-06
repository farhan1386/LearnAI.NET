using LearnAI.RAG.Interfaces;
using Microsoft.Extensions.AI;

namespace LearnAI.RAG.Services
{
    public class VectorService : IEmbeddingService
    {
        private readonly IEmbeddingGenerator<string, Embedding<float>> _generator;

        public VectorService(IEmbeddingGenerator<string, Embedding<float>> generator)
        {
            _generator = generator ?? throw new ArgumentNullException(nameof(generator));
        }

        public async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(text)) return ReadOnlyMemory<float>.Empty;

            var result = await _generator.GenerateAsync(text, cancellationToken: cancellationToken);
            return result.Vector;
        }

        public async Task<List<ReadOnlyMemory<float>>> GenerateEmbeddingsBatchAsync(List<string> texts, CancellationToken cancellationToken = default)
        {
            var vectors = new List<ReadOnlyMemory<float>>();
            if (texts == null || texts.Count == 0) return vectors;

            var results = await _generator.GenerateAsync(texts, cancellationToken: cancellationToken);
            foreach (var embedding in results)
            {
                vectors.Add(embedding.Vector);
            }

            return vectors;
        }
    }
}
