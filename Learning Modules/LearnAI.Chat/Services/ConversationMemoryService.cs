using LearnAI.Chat.Models;
using Microsoft.Extensions.AI;

namespace LearnAI.Chat.Services
{
    public class ConversationMemoryService
    {
        private readonly Dictionary<string, ConversationSession> _sessions = new();

        public ConversationSession GetSession(string sessionId)
        {
            if (!_sessions.ContainsKey(sessionId))
            {
                _sessions[sessionId] = new ConversationSession
                {
                    SessionId = sessionId,
                    Messages = new List<ChatMessage>()
                };
            }

            return _sessions[sessionId];
        }
    }
}