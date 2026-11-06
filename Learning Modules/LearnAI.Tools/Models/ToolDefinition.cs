using System;
using System.Collections.Generic;
using System.Text;

namespace LearnAI.Tools.Models
{
    public class ToolDefinition
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ParametersSchemaJson { get; set; } = string.Empty;
    }
}
