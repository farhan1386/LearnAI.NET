using LearnAI.Agents.Models;
using LearnAI.WebClients;
using Microsoft.AspNetCore.Mvc;

namespace LearnAI.Web.Controllers
{
    public class AgentsController : Controller
    {
        private readonly DynamicClient _dynamicClient;

        public AgentsController(DynamicClient dynamicClient)
        {
            _dynamicClient = dynamicClient;
        }

        public IActionResult Index()
        {
            return View(new AgentState());
        }

        [HttpPost]
        public async Task<IActionResult> Index(string goal, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(goal))
            {
                ModelState.AddModelError("goal", "Goal cannot be empty.");
                return View(new AgentState());
            }

            var state = await _dynamicClient.SendPostAsync<string, AgentState>("api/agents/execute", goal, cancellationToken);
            return View(state);
        }
    }
}