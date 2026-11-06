using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System;
using System.Threading.Tasks;

namespace LearnAI.Kernel.Orchestration
{
    public class MultiStepPlanner
    {
        private readonly Microsoft.SemanticKernel.Kernel _kernel;

        public MultiStepPlanner(Microsoft.SemanticKernel.Kernel kernel)
        {
            _kernel = kernel;
        }

        public async Task<string> ExecutePlanAsync(string userGoal)
        {
            var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

            var chatHistory = new ChatHistory();
            chatHistory.AddUserMessage(userGoal);

            var executionSettings = new OpenAIPromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            var response = await chatCompletionService.GetChatMessageContentAsync(
                chatHistory,
                executionSettings,
                _kernel);

            return response.ToString();
        }
    }
}