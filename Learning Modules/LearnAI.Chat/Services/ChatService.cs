using LearnAI.Chat.Interfaces;
using LearnAI.Chat.Models;
using Microsoft.Extensions.AI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LearnAI.Chat.Services
{
    public sealed class ChatService
    {
        private readonly IChatClientFactory _clientFactory;
        private readonly ConversationMemoryService _memory;
        private readonly TokenService _tokenService;

        public ChatService(
            IChatClientFactory clientFactory,
            ConversationMemoryService memory,
            TokenService tokenService)
        {
            _clientFactory = clientFactory;
            _memory = memory;
            _tokenService = tokenService;
        }

        private ChatOptions MapOptions(ChatRequest request)
        {
            return new ChatOptions
            {
                Temperature = request.Temperature,
                MaxOutputTokens = request.MaximumTokens > 0 ? request.MaximumTokens : null
            };
        }

        private List<ChatMessage> PrepareRequestMessages(ConversationSession session, ChatRequest request)
        {
            lock (session.SyncLock)
            {
                if (!session.Messages.Any())
                {
                    session.Messages.Add(new ChatMessage(ChatRole.System, request.SystemPrompt ?? "You are a helpful assistant."));
                }

                session.Messages.Add(new ChatMessage(ChatRole.User, request.Message!));
                ManageContextWindow(session, request.AIModel ?? "phi4");

                return new List<ChatMessage>(session.Messages);
            }
        }

        private void RollbackLastUserMessage(ConversationSession session)
        {
            lock (session.SyncLock)
            {
                if (session.Messages.Count > 0 && session.Messages[^1].Role == ChatRole.User)
                {
                    session.Messages.RemoveAt(session.Messages.Count - 1);
                }
            }
        }

        private void ManageContextWindow(ConversationSession session, string modelName)
        {
            int contextCeiling = modelName.ToLower() switch
            {
                "phi3" => 4000,
                "llama3.2" => 128000,
                _ => 163840
            };

            int maxAllowedInputTokens = contextCeiling - 2000;

            while (session.Messages.Count > 2)
            {
                int totalEstimatedTokens = session.Messages.Sum(m => _tokenService.EstimateTokens(m.Text ?? string.Empty));

                if (totalEstimatedTokens <= maxAllowedInputTokens)
                {
                    break;
                }

                int indexToRemove = session.Messages.FindIndex(m => m.Role != ChatRole.System);

                if (indexToRemove != -1)
                {
                    session.Messages.RemoveAt(indexToRemove);

                    if (indexToRemove < session.Messages.Count && session.Messages[indexToRemove].Role == ChatRole.Assistant)
                    {
                        session.Messages.RemoveAt(indexToRemove);
                    }
                }
                else
                {
                    break;
                }
            }
        }

        public async Task<LearnAI.Chat.Models.ChatResponse> AskAsync(ChatRequest request, CancellationToken cancellationToken = default)
        {
            var chatResponse = new LearnAI.Chat.Models.ChatResponse();
            var stopwatch = Stopwatch.StartNew();

            if (string.IsNullOrWhiteSpace(request.Message))
            {
                chatResponse.IsSuccess = false;
                chatResponse.ErrorMessage = "Input message cannot be null or empty.";
                return chatResponse;
            }

            var session = _memory.GetSession(request.SessionId ?? "default");

            try
            {
                var chatClient = _clientFactory.CreateClient(request.AIModel ?? string.Empty);
                var messagesSnapshot = PrepareRequestMessages(session, request);
                var options = MapOptions(request);

                var response = await chatClient.GetResponseAsync(messagesSnapshot, options, cancellationToken);
                var replyText = response.Text ?? string.Empty;

                lock (session.SyncLock)
                {
                    session.Messages.Add(new ChatMessage(ChatRole.Assistant, replyText));
                }

                chatResponse.ResponseText = replyText;
                chatResponse.IsSuccess = true;

                if (response.Usage != null)
                {
                    chatResponse.PromptTokens = (int)(response.Usage.InputTokenCount ?? 0);
                    chatResponse.CompletionTokens = (int)(response.Usage.OutputTokenCount ?? 0);
                    chatResponse.TotalTokens = (int)(response.Usage.TotalTokenCount ?? 0);
                }
            }
            catch (Exception ex)
            {
                RollbackLastUserMessage(session);
                chatResponse.IsSuccess = false;
                chatResponse.ErrorMessage = $"Failed to execute AI request: {ex.Message}";
            }
            finally
            {
                stopwatch.Stop();
                chatResponse.ExecutionTime = stopwatch.Elapsed;
            }

            return chatResponse;
        }

        public async IAsyncEnumerable<string> StreamAsync(ChatRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                yield break;
            }

            var chatClient = _clientFactory.CreateClient(request.AIModel ?? string.Empty);
            var session = _memory.GetSession(request.SessionId ?? "default");
            var messagesSnapshot = PrepareRequestMessages(session, request);
            var options = MapOptions(request);

            var responseBuilder = new StringBuilder();
            bool completed = false;

            try
            {
                await foreach (var update in chatClient.GetStreamingResponseAsync(messagesSnapshot, options, cancellationToken).WithCancellation(cancellationToken))
                {
                    if (!string.IsNullOrEmpty(update.Text))
                    {
                        responseBuilder.Append(update.Text);
                        yield return update.Text;
                    }
                }

                lock (session.SyncLock)
                {
                    session.Messages.Add(new ChatMessage(ChatRole.Assistant, responseBuilder.ToString()));
                }

                completed = true;
            }
            finally
            {
                if (!completed)
                {
                    RollbackLastUserMessage(session);
                }
            }
        }
    }
}