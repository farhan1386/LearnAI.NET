using LearnAI.Web.Models;
using LearnAI.WebClients;
using Microsoft.AspNetCore.Mvc;

namespace LearnAI.Web.Controllers
{
    public class ToolsController : Controller
    {
        private readonly DynamicClient _dynamicClient;

        public ToolsController(DynamicClient dynamicClient)
        {
            _dynamicClient = dynamicClient;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new ToolsViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Execute(ToolsViewModel model)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(model.Prompt))
            {
                model.ErrorMessage = "Please provide a valid execution prompt.";
                return View("Index", model);
            }

            try
            {
                var payload = new
                {
                    prompt = model.Prompt,
                    selectionMode = model.SelectionMode,
                    targetToolName = model.TargetToolName
                };

                var result = await _dynamicClient.SendPostAsync<object, ToolExecutionResult>("api/tools/execute", payload);

                if (result != null && result.Success)
                {
                    model.ExecutionResult = result.Output;
                    model.ErrorMessage = null;
                }
                else
                {
                    model.ErrorMessage = result?.Error ?? "The execution returned an unsuccessful payload response.";
                    model.ExecutionResult = null;
                }
            }
            catch (Exception ex)
            {
                model.ErrorMessage = $"Failed to communicate with the tools engine gateway: {ex.Message}";
                model.ExecutionResult = null;
            }

            return View("Index", model);
        }
    }

    public class ToolExecutionResult
    {
        public bool Success { get; set; }
        public string Output { get; set; } = string.Empty;
        public string? Error { get; set; }
    }
}