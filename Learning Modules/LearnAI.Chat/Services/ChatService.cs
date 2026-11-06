using LearnAI.Chat.Interfaces;
using LearnAI.Chat.Models;
using Microsoft.Extensions.AI;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace LearnAI.Chat.Services
{
    public sealed class ChatService
    {
        private readonly IChatClientFactory _clientFactory;
        private readonly ConversationMemoryService _memory;

        public ChatService(IChatClientFactory clientFactory, ConversationMemoryService memory)
        {
            _clientFactory = clientFactory;
            _memory = memory;
        }

        private ChatOptions MapOptions(ChatRequest request)
        {
            return new ChatOptions
            {
                Temperature = request.Temperature,
                MaxOutputTokens = request.MaximumTokens > 0 ? request.MaximumTokens : null
            };
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
                int totalEstimatedTokens = 0;
                foreach (var message in session.Messages)
                {
                    totalEstimatedTokens += (message.Text?.Length ?? 0) / 4;
                }

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

        public async Task<LearnAI.Chat.Models.ChatResponse> AskAsync(ChatRequest request)
        {
            var chatResponse = new LearnAI.Chat.Models.ChatResponse();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                if (string.IsNullOrWhiteSpace(request.Message))
                {
                    chatResponse.IsSuccess = false;
                    chatResponse.ErrorMessage = "Input message cannot be null or empty.";
                    return chatResponse;
                }

                var chatClient = _clientFactory.CreateClient(request.AIModel ?? string.Empty);
                var session = _memory.GetSession(request.SessionId ?? "default");

                if (!session.Messages.Any())
                {
                    session.Messages.Add(new ChatMessage(ChatRole.System, request.SystemPrompt ?? "You are a helpful assistant."));
                }

                session.Messages.Add(new ChatMessage(ChatRole.User, request.Message));

                ManageContextWindow(session, request.AIModel ?? "phi4");

                var options = MapOptions(request);
                var response = await chatClient.GetResponseAsync(session.Messages, options);

                session.Messages.Add(new ChatMessage(ChatRole.Assistant, response.Text ?? string.Empty));

                chatResponse.ResponseText = response.Text ?? string.Empty;
                chatResponse.IsSuccess = true;

                if (response.Usage != null)
                {
                    chatResponse.PromptTokens = (int)response.Usage.InputTokenCount;
                    chatResponse.CompletionTokens = (int)response.Usage.OutputTokenCount;
                    chatResponse.TotalTokens = (int)response.Usage.TotalTokenCount;
                }
            }
            catch (Exception ex)
            {
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

            if (!session.Messages.Any())
            {
                session.Messages.Add(new ChatMessage(ChatRole.System, request.SystemPrompt ?? "You are a helpful assistant."));
            }

            session.Messages.Add(new ChatMessage(ChatRole.User, request.Message));

            ManageContextWindow(session, request.AIModel ?? "phi4");

            string fullResponse = string.Empty;
            var options = MapOptions(request);

            await foreach (var update in chatClient.GetStreamingResponseAsync(session.Messages, options, cancellationToken: cancellationToken).WithCancellation(cancellationToken))
            {
                if (!string.IsNullOrEmpty(update.Text))
                {
                    fullResponse += update.Text;
                    yield return update.Text;
                }
            }

            session.Messages.Add(new ChatMessage(ChatRole.Assistant, fullResponse));
        }
    }
}