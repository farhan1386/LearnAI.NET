using System;
using System.Collections.Generic;
using System.Text;

namespace LearnAI.Agents.Models
{
    public class AgentPlan
    {
        public string? OriginalGoal { get; set; }
        public List<PlanTask> Tasks { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
