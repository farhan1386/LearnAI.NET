using Microsoft.Extensions.AI;
using System;
using System.Collections.Concurrent;

namespace LearnAI.Chat.Interfaces
{
    public interface IChatClientFactory
    {
        IChatClient CreateClient(string modelName);
    }

    public sealed class OllamaChatClientFactory : IChatClientFactory, IDisposable
    {
        private readonly ConcurrentDictionary<string, IChatClient> _clients = new(StringComparer.OrdinalIgnoreCase);
        private readonly Uri _endpoint = new("http://localhost:11434");

        public IChatClient CreateClient(string modelName)
        {
            string targetModel = string.IsNullOrWhiteSpace(modelName) ? "phi4" : modelName;
            return _clients.GetOrAdd(targetModel, model => new OllamaChatClient(_endpoint, model));
        }

        public void Dispose()
        {
            foreach (var client in _clients.Values)
            {
                if (client is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            _clients.Clear();
        }
    }
}
