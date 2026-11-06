using LearnAI.Agents.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LearnAI.Agents.Interfaces
{
    public interface IAgentPlanner
    {
        Task<AgentPlan> CreatePlanAsync(string goal, CancellationToken cancellationToken = default);
    }
}
