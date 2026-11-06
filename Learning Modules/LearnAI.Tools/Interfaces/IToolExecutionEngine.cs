using LearnAI.Tools.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks; 

namespace LearnAI.Tools.Interfaces
{
    public interface IToolExecutionEngine
    {
        void RegisterTool(IAITool tool);
        Task<string> ProcessExecutionAsync(string userPrompt, object vendorChatOptions, ToolSelectionMode selectionMode, string? targetToolName = null);
        Task<string> ExecuteToolWithMiddlewareAsync(string toolName, string argumentsJson);
    }
}