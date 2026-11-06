using LearnAI.RAG.Models;

namespace LearnAI.RAG.Services
{
    public class TextChunkerService
    {
        public List<TextChunk> SplitIntoChunks(string text, int chunkSize = 500, int chunkOverlap = 50)
        {
            var chunks = new List<TextChunk>();
            if (string.IsNullOrWhiteSpace(text)) return chunks;

            int textLength = text.Length;
            int start = 0;
            int index = 0;

            while (start < textLength)
            {
                int length = Math.Min(chunkSize, textLength - start);
                string content = text.Substring(start, length);

                chunks.Add(new TextChunk
                {
                    Content = content,
                    StartCharacterIndex = start,
                    EndCharacterIndex = start + length,
                    ChunkIndex = index++
                });

                start += (chunkSize - chunkOverlap);
                if (start >= textLength || chunkSize >= textLength - start + chunkOverlap) break;
            }

            return chunks;
        }
    }
}