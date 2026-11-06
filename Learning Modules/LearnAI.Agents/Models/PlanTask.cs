using System;
using System.Collections.Generic;
using System.Text;

namespace LearnAI.Agents.Models
{
    public class PlanTask
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        public string? TargetAgent { get; set; }
        public bool IsCompleted { get; set; }
        public string? OutputSummary { get; set; }
    }
}
