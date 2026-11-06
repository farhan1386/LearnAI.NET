using System;
using System.Collections.Generic;
using System.Text;

namespace LearnAI.Agents.Models
{
    public class AgentState
    {
        public Guid ExecutionId { get; set; }
        public string UserGoal { get; set; } = string.Empty;
        public string FinalAnswer { get; set; } = string.Empty;
        public bool IsComplete { get; set; }
        public int MaxSteps { get; set; }
        public List<AgentStep> Steps { get; set; }
        public Dictionary<string, object> Variables { get; set; }
        public AgentPlan ExecutionPlan { get; set; }
    }
}
