using LearnAI.Chat.Models;
using LearnAI.WebClients;
using Microsoft.AspNetCore.Mvc;

namespace LearnAI.Web.Controllers
{
    public class ChatController : Controller
    {
        private readonly DynamicClient _apiClient;

        public ChatController(DynamicClient apiClient)
        {
            _apiClient = apiClient;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task Stream(string question, string? sessionId, string model = "phi4", string? systemPrompt = null, float temperature = 0.7f, int maxTokens = 1000, CancellationToken cancellationToken = default)
        {
            Response.ContentType = "text/event-stream";
            Response.Headers.CacheControl = "no-cache";
            Response.Headers.Connection = "keep-alive";

            string url = $"chat/stream?question={Uri.EscapeDataString(question ?? string.Empty)}&model={Uri.EscapeDataString(model ?? string.Empty)}&sessionId={Uri.EscapeDataString(sessionId ?? string.Empty)}&temperature={temperature}&maxTokens={maxTokens}";

            try
            {
                using var response = await _apiClient.SendGetStreamAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();

                using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var reader = new StreamReader(stream);

                while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync(cancellationToken);
                    if (string.IsNullOrEmpty(line)) continue;

                    string cleanChunk = line;
                    if (cleanChunk.StartsWith("data: "))
                    {
                        cleanChunk = cleanChunk.Substring(6);
                    }

                    await Response.WriteAsync($"data: {cleanChunk}\n\n", cancellationToken);
                    await Response.Body.FlushAsync(cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(string userMessage, string selectedModel, string systemPrompt, string sessionId, float temperature = 0.7f, int maxTokens = 1000)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
            {
                return Json(new
                {
                    success = false,
                    response = "Please enter a valid message."
                });
            }

            try
            {
                string model = string.IsNullOrWhiteSpace(selectedModel) ? "phi4" : selectedModel;
                string url = $"chat?question={Uri.EscapeDataString(userMessage)}&model={Uri.EscapeDataString(model)}&sessionId={Uri.EscapeDataString(sessionId ?? string.Empty)}&temperature={temperature}&maxTokens={maxTokens}";

                var apiResult = await _apiClient.SendGetAsync<ChatResponse>(url);

                return Json(new
                {
                    success = true,
                    response = apiResult?.ResponseText ?? string.Empty
                });
            }
            catch (HttpRequestException ex)
            {
                return Json(new
                {
                    success = false,
                    response = $"Failed to reach backend API: {ex.Message}"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    response = $"An unexpected error occurred: {ex.Message}"
                });
            }
        }
    }
}