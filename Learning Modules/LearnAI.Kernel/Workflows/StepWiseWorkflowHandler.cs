using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace LearnAI.Kernel.Workflows
{
    public class StepWiseWorkflowHandler
    {
        private readonly Microsoft.SemanticKernel.Kernel _kernel;

        public StepWiseWorkflowHandler(Microsoft.SemanticKernel.Kernel kernel)
        {
            _kernel = kernel;
        }

        public async Task<string> RunWorkflowAsync(string input, string initialState)
        {
            var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
            var executionSettings = new OpenAIPromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            var chatHistory = new ChatHistory();
            string currentState = initialState;
            bool workflowComplete = false;
            int currentStep = 0;
            int maxSteps = 5;

            while (!workflowComplete && currentStep < maxSteps)
            {
                currentStep++;

                chatHistory.Clear();
                chatHistory.AddSystemMessage($"You are managing a conditional workflow state machine. Current state: {currentState}. Decide whether to execute a tool or switch the state based on the input.");
                chatHistory.AddUserMessage(input);

                var response = await chatCompletionService.GetChatMessageContentAsync(chatHistory, executionSettings, _kernel);

                string content = response.ToString();

                if (content.Contains("workflow_complete", StringComparison.OrdinalIgnoreCase))
                {
                    workflowComplete = true;
                }
                else if (content.Contains("transition_to:", StringComparison.OrdinalIgnoreCase))
                {
                    var lines = content.Split('\n');
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("transition_to:", StringComparison.OrdinalIgnoreCase))
                        {
                            currentState = line.Split(':')[1].Trim();
                            break;
                        }
                    }
                }
                else
                {
                    return content;
                }
            }

            return $"Workflow execution completed. Final state reached: {currentState}";
        }
    }
}