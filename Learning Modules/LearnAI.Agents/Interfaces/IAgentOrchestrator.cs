using LearnAI.Agents.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LearnAI.Agents.Interfaces
{
    public interface IAgentOrchestrator
    {
        Task<AgentState> ExecuteAsync(string userGoal, CancellationToken cancellationToken = default);

    }
}
