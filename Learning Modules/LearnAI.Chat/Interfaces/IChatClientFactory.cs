using Microsoft.Extensions.AI;
using System;

namespace LearnAI.Chat.Interfaces
{
    public interface IChatClientFactory
    {
        IChatClient CreateClient(string modelName);
    }

    public class OllamaChatClientFactory : IChatClientFactory
    {
        public IChatClient CreateClient(string modelName)
        {
            string targetModel = string.IsNullOrWhiteSpace(modelName) ? "phi4" : modelName;
            return new OllamaChatClient(new Uri("http://localhost:11434"), targetModel);
        }
    }
}