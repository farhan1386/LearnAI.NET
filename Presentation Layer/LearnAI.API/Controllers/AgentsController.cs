using Microsoft.AspNetCore.Mvc;
using LearnAI.Agents.Interfaces;
using LearnAI.Agents.Models;

namespace LearnAI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgentsController : ControllerBase
    {
        private readonly IAgentOrchestrator _orchestrator;

        public AgentsController(IAgentOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [HttpPost("execute")]
        public async Task<ActionResult<AgentState>> ExecuteGoal([FromBody] string goal, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(goal))
            {
                return BadRequest("Goal cannot be empty.");
            }

            var result = await _orchestrator.ExecuteAsync(goal, cancellationToken);
            return Ok(result);
        }
    }
}
