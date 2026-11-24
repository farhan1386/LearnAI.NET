using Microsoft.Extensions.AI;
using System.Collections.Generic;

namespace LearnAI.Chat.Models
{
    public class ConversationSession
    {
        public string? SessionId { get; set; }
        public List<ChatMessage> Messages { get; set; } = new();
        public object SyncLock { get; } = new();
    }
}