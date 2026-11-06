using LearnAI.Tools.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LearnAI.Tools.Interfaces
{
    public interface IToolMiddleware
    {
        Task<string> InvokeAsync(ToolExecutionContext context, Func<Task<string>> next);
    }
}
