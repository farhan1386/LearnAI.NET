using System;
using System.Collections.Generic;
using System.Text;

namespace LearnAI.Chat.Models
{
    public class TokenAnalysis
    {
        public string? Text { get; set; } 
        public int TokenCount { get; set; }
        public int CharacterCount { get; set; }
        public int WordCount { get; set; }
    }
}
