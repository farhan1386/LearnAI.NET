using System;
using System.Collections.Generic;
using System.Text;

namespace LearnAI.RAG.Models
{
    public class TextChunk
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Content { get; set; } = string.Empty;
        public int StartCharacterIndex { get; set; }
        public int EndCharacterIndex { get; set; }
        public int ChunkIndex { get; set; }
    }
}
