using System;
using System.Collections.Generic;
using System.Text;

namespace LearnAI.RAG.Models
{
    public class VectorDocument
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Content { get; set; } = string.Empty;
        public ReadOnlyMemory<float> Embedding { get; set; }
        public string SourceDocumentName { get; set; } = string.Empty;
        public int ChunkIndex { get; set; }
    }
}
