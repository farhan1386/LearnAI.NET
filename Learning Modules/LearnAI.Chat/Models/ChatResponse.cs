using System;
using System.Collections.Generic;
using System.Text;

namespace LearnAI.Chat.Models
{
    public class ChatResponse
    {
        public string? ResponseText { get; set; }
        public TimeSpan ExecutionTime { get; set; }
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens { get; set; }
    }
}