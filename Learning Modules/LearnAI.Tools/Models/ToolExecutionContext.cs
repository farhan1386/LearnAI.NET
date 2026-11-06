using LearnAI.Tools.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace LearnAI.Tools.Models
{
    public class ToolExecutionContext
    {
        public IAITool Tool { get; }
        public string RawArgumentsJson { get; set; }
        public Dictionary<string, object> Metadata { get; } = new();

        public ToolExecutionContext(IAITool tool, string rawArgumentsJson)
        {
            Tool = tool;
            RawArgumentsJson = rawArgumentsJson;
        }
    }
}
