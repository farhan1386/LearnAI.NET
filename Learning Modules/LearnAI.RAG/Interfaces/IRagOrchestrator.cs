using System;
using System.Collections.Generic;
using System.Text;

namespace LearnAI.RAG.Interfaces
{
    public interface IRagOrchestrator
    {
        Task IngestDocumentAsync(string content, string documentName, CancellationToken cancellationToken = default);
        Task<string> QueryAsync(string userPrompt, CancellationToken cancellationToken = default);
    }
}
