using System;
using System.Collections.Generic;
using System.Text;

namespace LearnAI.RAG.Models
{
    public class VectorQueryResult
    {
        public VectorDocument Document { get; set; } = new();
        public float SimilarityScore { get; set; }
    }
}
