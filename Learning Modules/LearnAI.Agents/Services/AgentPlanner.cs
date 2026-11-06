using LearnAI.Agents.Interfaces;
using LearnAI.Agents.Models;
using Microsoft.Extensions.Logging;

namespace LearnAI.Agents.Services
{
    public class AgentPlanner : IAgentPlanner
    {
        private readonly ILogger<AgentPlanner> _logger;

        public AgentPlanner(ILogger<AgentPlanner> logger)
        {
            _logger = logger;
        }

        public async Task<AgentPlan> CreatePlanAsync(string goal, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Decomposing complex goal into a strategic plan: {Goal}", goal);

            var plan = new AgentPlan
            {
                OriginalGoal = goal,
                CreatedAt = DateTime.UtcNow,
                Tasks = new List<PlanTask>()
            };

            plan.Tasks.Add(new PlanTask { Id = 1, Description = "Gather and aggregate primary resource documentation.", TargetAgent = "ResearchAgent", IsCompleted = false });
            plan.Tasks.Add(new PlanTask { Id = 2, Description = "Synthesize collected intelligence into a comprehensive report draft.", TargetAgent = "WriterAgent", IsCompleted = false });
            plan.Tasks.Add(new PlanTask { Id = 3, Description = "Audit document against criteria rules and verify claims.", TargetAgent = "ReviewAgent", IsCompleted = false });

            _logger.LogInformation("Successfully generated execution plan with {Count} sub-tasks.", plan.Tasks.Count);

            return await Task.FromResult(plan);
        }
    }
}