using Microsoft.Extensions.AI;
using System;
using System.Collections.Generic;
using System.Text;

namespace LearnAI.Chat.Models
{
    public class ConversationSession
    {
        public string? SessionId { get; set; }
        public List<ChatMessage> Messages { get; set; }
    }
}