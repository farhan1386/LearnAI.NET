using LearnAI.RAG.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LearnAI.RAG.Interfaces
{
    public interface IVectorDatabase
    {
        Task SaveDocumentAsync(VectorDocument document, CancellationToken cancellationToken = default);
        Task<List<VectorQueryResult>> SearchSimilarAsync(ReadOnlyMemory<float> queryEmbedding, int limit = 5, CancellationToken cancellationToken = default);
    }
}
