using LearnAI.Tools.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LearnAI.Tools.Interfaces
{
    public interface IAITool
    {
        ToolDefinition Definition { get; }
        Task<string> ExecuteAsync(string argumentsJson);
    }
}
