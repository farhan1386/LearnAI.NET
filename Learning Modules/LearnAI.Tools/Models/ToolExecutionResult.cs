using System;
using System.Collections.Generic;
using System.Text;

namespace LearnAI.Tools.Models
{
    public class ToolExecutionResult<T>
    {
        public bool IsSuccess { get; set; }
        public T? Data { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public int AttemptsTaken { get; set; }
        public string RawPayload { get; set; } = string.Empty;
    }
}
