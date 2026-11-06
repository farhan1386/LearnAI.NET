using LearnAI.WebClients;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using LearnAI.Gateway.Models;

namespace LearnAI.Web.Controllers
{
    public class GatewayController : Controller
    {
        private readonly DynamicClient _dynamicClient;

        public GatewayController(DynamicClient dynamicClient)
        {
            _dynamicClient = dynamicClient;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Compare(string prompt, string? modelName = null)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                return Json(new { success = false, message = "Benchmark prompt cannot be empty." });
            }

            try
            {
                string url = $"api/gateway/compare?prompt={Uri.EscapeDataString(prompt)}";

                if (!string.IsNullOrWhiteSpace(modelName))
                {
                    url += $"&modelName={Uri.EscapeDataString(modelName)}";
                }

                var report = await _dynamicClient.SendGetAsync<object>(url);

                return Json(new { success = true, metrics = report });
            }
            catch (HttpRequestException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An unexpected error occurred: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Execute([FromBody] GatewayRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Prompt))
            {
                return Json(new { success = false, message = "Payload properties configuration is invalid." });
            }

            try
            {
                string rawJson = await _dynamicClient.SendPostReturnStringAsync("api/gateway/switch", request);

                var options = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var parsedResult = System.Text.Json.JsonSerializer.Deserialize<GatewayResponseDto>(rawJson, options);

                return Json(new { success = true, data = parsedResult?.Response ?? string.Empty });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private class GatewayResponseDto
        {
            public string Response { get; set; } = string.Empty;
        }
    }
}