using System;
using System.Collections.Generic;
using System.Text;

namespace LearnAI.RAG.Interfaces
{
    public interface IEmbeddingService
    {
        Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);
        Task<List<ReadOnlyMemory<float>>> GenerateEmbeddingsBatchAsync(List<string> texts, CancellationToken cancellationToken = default);
    }
}
