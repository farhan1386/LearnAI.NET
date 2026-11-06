using System;
using System.Collections.Generic;
using System.Text;

namespace LearnAI.Agents.Models
{
    public class AgentStep
    {
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Thought { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public Dictionary<string, object> ActionInput { get; set; }
        public string Observation { get; set; } = string.Empty;
    }
}
