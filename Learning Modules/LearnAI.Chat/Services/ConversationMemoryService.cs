using LearnAI.Chat.Models;
using System.Collections.Concurrent;

namespace LearnAI.Chat.Services
{
    public class ConversationMemoryService
    {
        private readonly ConcurrentDictionary<string, ConversationSession> _sessions = new();

        public ConversationSession GetSession(string sessionId)
        {
            return _sessions.GetOrAdd(sessionId, id => new ConversationSession
            {
                SessionId = id
            });
        }
    }
}