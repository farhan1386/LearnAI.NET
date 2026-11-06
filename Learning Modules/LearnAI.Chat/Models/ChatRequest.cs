using System;
using System.Collections.Generic;
using System.Text;

namespace LearnAI.Chat.Models
{
    public class ChatRequest
    {
        public string? Message { get; set; }
        public string? SystemPrompt { get; set; }
        public string? AIModel { get; set; }
        public float Temperature { get; set; }
        public int MaximumTokens { get; set; }
        public string? SessionId { get; set; }
    }
}