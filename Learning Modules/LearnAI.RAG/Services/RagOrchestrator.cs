using LearnAI.RAG.Interfaces;
using LearnAI.RAG.Models;
using Microsoft.Extensions.AI;
using System.Text;

namespace LearnAI.RAG.Services
{
    public class RagOrchestrator : IRagOrchestrator
    {
        private readonly TextChunkerService _chunker;
        private readonly IEmbeddingService _embeddingService;
        private readonly IVectorDatabase _vectorDb;
        private readonly IChatClient _chatClient;

        public RagOrchestrator(TextChunkerService chunker, IEmbeddingService embeddingService, IVectorDatabase vectorDb, IChatClient chatClient)
        {
            _chunker = chunker;
            _embeddingService = embeddingService;
            _vectorDb = vectorDb;
            _chatClient = chatClient;
        }

        public async Task IngestDocumentAsync(string content, string documentName, CancellationToken cancellationToken = default)
        {
            var chunks = _chunker.SplitIntoChunks(content);

            foreach (var chunk in chunks)
            {
                var vector = await _embeddingService.GenerateEmbeddingAsync(chunk.Content, cancellationToken);

                var doc = new VectorDocument
                {
                    Content = chunk.Content,
                    Embedding = vector,
                    SourceDocumentName = documentName,
                    ChunkIndex = chunk.ChunkIndex
                };

                await _vectorDb.SaveDocumentAsync(doc, cancellationToken);
            }
        }

        public async Task<string> QueryAsync(string userPrompt, CancellationToken cancellationToken = default)
        {
            var queryVector = await _embeddingService.GenerateEmbeddingAsync(userPrompt, cancellationToken);
            var matches = await _vectorDb.SearchSimilarAsync(queryVector, limit: 3, cancellationToken);

            var contextBuilder = new StringBuilder();
            foreach (var match in matches)
            {
                contextBuilder.AppendLine(match.Document.Content);
            }

            var sysInstruction = $"Answer the query using this context:\n{contextBuilder}";
            var history = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.System, sysInstruction),
                new ChatMessage(ChatRole.User, userPrompt)
            };

            var response = await _chatClient.GetResponseAsync(history, cancellationToken: cancellationToken);
            return response.Text ?? string.Empty;
        }
    }
}