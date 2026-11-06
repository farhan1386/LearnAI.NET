using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LearnAI.WebClients;
using Microsoft.AspNetCore.Mvc;

namespace LearnAI.Web.Controllers
{
    public class MCPController : Controller
    {
        private readonly DynamicClient _dynamicClient;

        public MCPController(DynamicClient dynamicClient)
        {
            _dynamicClient = dynamicClient;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Execute(string serverName, string toolName, string arguments, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(serverName))
            {
                ModelState.AddModelError("serverName", "Server name cannot be empty.");
                return View("Index");
            }

            if (string.IsNullOrWhiteSpace(toolName))
            {
                ModelState.AddModelError("toolName", "Tool name cannot be empty.");
                return View("Index");
            }

            var parsedArguments = new Dictionary<string, object>();
            if (!string.IsNullOrWhiteSpace(arguments))
            {
                try
                {
                    parsedArguments = JsonSerializer.Deserialize<Dictionary<string, object>>(arguments) ?? new();
                }
                catch
                {
                    ModelState.AddModelError("arguments", "Invalid JSON object provided for arguments.");
                    return View("Index");
                }
            }

            try
            {
                string requestUri = $"api/mcp/tools/execute?serverName={Uri.EscapeDataString(serverName)}&toolName={Uri.EscapeDataString(toolName)}";

                string rawTextResult = await _dynamicClient.SendPostReturnStringAsync(requestUri, parsedArguments, cancellationToken);

                using var document = JsonDocument.Parse(rawTextResult);
                var options = new JsonSerializerOptions { WriteIndented = true };

                ViewBag.Result = JsonSerializer.Serialize(document.RootElement, options);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"API Error: {ex.Message}");
            }

            return View("Index");
        }
    }
}