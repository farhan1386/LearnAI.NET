using LearnAI.Agents.Interfaces;
using LearnAI.Agents.Models;
using LearnAI.Agents.ResearchAgent;
using LearnAI.Agents.WriterAgent;
using LearnAI.Agents.ReviewAgent;
using Microsoft.Extensions.Logging;

namespace LearnAI.Agents.Services
{
    public class AgentOrchestrator : IAgentOrchestrator
    {
        private readonly ILogger<AgentOrchestrator> _logger;
        private readonly IAgentPlanner _planner;
        private readonly ResearchAgentService _researchAgent;
        private readonly WriterAgentService _writerAgent;
        private readonly ReviewAgentService _reviewAgent;
        private readonly Dictionary<string, List<string>> _episodicMemoryStore;
        private readonly Dictionary<string, Func<Dictionary<string, object>, Task<string>>> _agentChannelRouters;

        public AgentOrchestrator(
            ILogger<AgentOrchestrator> logger,
            IAgentPlanner planner,
            ResearchAgentService researchAgent,
            WriterAgentService writerAgent,
            ReviewAgentService reviewAgent)
        {
            _logger = logger;
            _planner = planner;
            _researchAgent = researchAgent;
            _writerAgent = writerAgent;
            _reviewAgent = reviewAgent;

            _episodicMemoryStore = new Dictionary<string, List<string>>();
            _agentChannelRouters = new Dictionary<string, Func<Dictionary<string, object>, Task<string>>>();

            InitializeMultiAgentCommunicationChannels();
        }

        public async Task<AgentState> ExecuteAsync(string userGoal, CancellationToken cancellationToken = default)
        {
            var state = new AgentState
            {
                ExecutionId = Guid.NewGuid(),
                UserGoal = userGoal,
                FinalAnswer = string.Empty,
                IsComplete = false,
                MaxSteps = 10,
                Steps = new List<AgentStep>(),
                Variables = new Dictionary<string, object>(),
                ExecutionPlan = new AgentPlan { Tasks = new List<PlanTask>() }
            };

            state.ExecutionPlan = await _planner.CreatePlanAsync(userGoal, cancellationToken);

            _logger.LogInformation("Starting agentic loop execution for Goal: {Goal}", userGoal);

            var contextKey = $"Goal_{userGoal.GetHashCode()}";
            var historicalContext = RecallLongTermMemory(contextKey);

            while (!state.IsComplete && state.Steps.Count < state.MaxSteps)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var currentStepNumber = state.Steps.Count + 1;
                _logger.LogInformation("Processing Agent Step #{StepNumber}...", currentStepNumber);

                var currentTask = state.ExecutionPlan.Tasks.FirstOrDefault(t => !t.IsCompleted);

                if (currentTask == null)
                {
                    state.FinalAnswer = "All planned sub-tasks have been fully executed and verified.";
                    state.IsComplete = true;
                    break;
                }

                var contextualPromptBase = historicalContext.Count > 0
                    ? $"[Recalled Experience: {string.Join(" | ", historicalContext)}] "
                    : string.Empty;

                var currentStep = new AgentStep
                {
                    Id = Guid.NewGuid(),
                    Timestamp = DateTime.UtcNow,
                    Thought = $"{contextualPromptBase}ReAct Loop Step {currentStepNumber}: Processing task objective '{currentTask.Description}'. Routing control boundary to team cell specialization channel.",
                    Action = currentTask.TargetAgent ?? string.Empty,
                    ActionInput = new Dictionary<string, object> { { "taskDescription", currentTask.Description ?? string.Empty } },
                    Observation = string.Empty
                };

                _logger.LogInformation("Polymorphic Role Handoff: Dispatching execution down route channel for: {Action}", currentStep.Action);

                currentStep.Observation = await RouteToAgentChannelAsync(currentStep.Action, currentStep.ActionInput);

                state.Steps.Add(currentStep);

                bool isStepValid = EvaluateIntermediateOutput(currentStep);

                if (isStepValid)
                {
                    bool selfReflectionPassed = ExecuteSelfReflectionAudit(currentStep, currentTask);

                    if (selfReflectionPassed)
                    {
                        currentTask.IsCompleted = true;
                        currentTask.OutputSummary = currentStep.Observation;

                        PersistEpisodicLog(contextKey, $"Task {currentTask.Id} Completed via {currentTask.TargetAgent}: {currentStep.Observation}");

                        _logger.LogInformation("Step {StepNumber} successfully cleared self-reflection validation audit.", currentStepNumber);
                    }
                    else
                    {
                        _logger.LogWarning("Step {StepNumber} failed self-reflection check. Requiring structural correction iteration loop.", currentStepNumber);
                        currentStep.Observation += " [Self-Reflection Correction: Output flagged for quality adjustment and refinement]";
                    }
                }
                else
                {
                    _logger.LogWarning("Step {StepNumber} failed structural format checks.", currentStepNumber);
                }
            }

            _logger.LogInformation("Agentic loop finished. Total steps taken: {Count}", state.Steps.Count);
            return state;
        }

        private void InitializeMultiAgentCommunicationChannels()
        {
            _agentChannelRouters["ResearchAgent"] = async (inputs) =>
            {
                var desc = inputs.TryGetValue("taskDescription", out var v) ? v.ToString() : string.Empty;
                return await _researchAgent.ProcessResearchAsync(desc ?? string.Empty);
            };

            _agentChannelRouters["WriterAgent"] = async (inputs) =>
            {
                var desc = inputs.TryGetValue("taskDescription", out var v) ? v.ToString() : string.Empty;
                return await _writerAgent.ProcessDraftAsync(desc ?? string.Empty);
            };

            _agentChannelRouters["ReviewAgent"] = async (inputs) =>
            {
                var desc = inputs.TryGetValue("taskDescription", out var v) ? v.ToString() : string.Empty;
                return await _reviewAgent.ExecuteAuditAsync(desc ?? string.Empty);
            };
        }

        private async Task<string> RouteToAgentChannelAsync(string agentRole, Dictionary<string, object> inputs)
        {
            if (_agentChannelRouters.TryGetValue(agentRole, out var channelExecutor))
            {
                return await channelExecutor(inputs);
            }

            _logger.LogWarning("Target handoff channel not found for role: {Role}. Falling back to default system cell route.", agentRole);
            return $"Fallback Channel Output: Baseline process handling complete for {agentRole}.";
        }

        private List<string> RecallLongTermMemory(string key)
        {
            if (_episodicMemoryStore.TryGetValue(key, out var memories))
            {
                _logger.LogInformation("Recalled {Count} episodic long-term memories for key: {Key}", memories.Count, key);
                return memories;
            }
            return new List<string>();
        }

        private void PersistEpisodicLog(string key, string memoryEntry)
        {
            if (!_episodicMemoryStore.ContainsKey(key))
            {
                _episodicMemoryStore[key] = new List<string>();
            }
            _episodicMemoryStore[key].Add(memoryEntry);
            _logger.LogInformation("Persisted new episodic memory index string to memory storage.");
        }

        private bool ExecuteSelfReflectionAudit(AgentStep step, PlanTask task)
        {
            if (task.TargetAgent == "ReviewAgent" && !step.Observation.Contains("100%"))
            {
                return false;
            }

            if (task.TargetAgent == "WriterAgent" && !step.Observation.Contains("markdown"))
            {
                return false;
            }

            return step.Observation.Length > 20;
        }

        private bool EvaluateIntermediateOutput(AgentStep step)
        {
            if (string.IsNullOrWhiteSpace(step.Thought) || string.IsNullOrWhiteSpace(step.Observation))
            {
                return false;
            }

            return step.Thought.Contains("ReAct") && step.Observation.Length > 0;
        }
    }
}
