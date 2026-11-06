using LearnAI.Chat.Models;
using LearnAI.Chat.Services;
using Microsoft.Extensions.AI;
using System;
using Xunit;

namespace LearnAI.AITests.ChatServices
{
    public class ConversationMemoryService_Tests
    {
        private readonly ConversationMemoryService _memoryService;

        public ConversationMemoryService_Tests()
        {
            _memoryService = new ConversationMemoryService();
        }

        [Fact]
        public void GetSession_WhenSessionDoesNotExist_ShouldCreateAndReturnNewSession()
        {
            string newSessionId = Guid.NewGuid().ToString();

            var session = _memoryService.GetSession(newSessionId);

            Assert.NotNull(session);
            Assert.Equal(newSessionId, session.SessionId);
            Assert.NotNull(session.Messages);
            Assert.Empty(session.Messages);
        }

        [Fact]
        public void GetSession_WhenSessionExists_ShouldReturnSameInstance()
        {
            string existingSessionId = "session-123";
            var firstCallResult = _memoryService.GetSession(existingSessionId);

            firstCallResult.Messages.Add(new ChatMessage(ChatRole.User, "Hello Engine"));

            var secondCallResult = _memoryService.GetSession(existingSessionId);

            Assert.NotNull(secondCallResult);
            Assert.Same(firstCallResult, secondCallResult);
            Assert.Equal(existingSessionId, secondCallResult.SessionId);
            Assert.Single(secondCallResult.Messages);
            Assert.Equal("Hello Engine", secondCallResult.Messages[0].Text);
        }

        [Fact]
        public void GetSession_ShouldMaintainIsolationBetweenDifferentSessions()
        {
            string sessionA = "Id-A";
            string sessionB = "Id-B";

            var instanceA = _memoryService.GetSession(sessionA);
            instanceA.Messages.Add(new ChatMessage(ChatRole.User, "Query A"));

            var instanceB = _memoryService.GetSession(sessionB);

            Assert.NotSame(instanceA, instanceB);
            Assert.Empty(instanceB.Messages);
        }
    }
}