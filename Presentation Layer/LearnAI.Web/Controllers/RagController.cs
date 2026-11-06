using LearnAI.WebClients;
using Microsoft.AspNetCore.Mvc;

namespace LearnAI.Web.Controllers
{
    public class RagController : Controller
    {
        private readonly DynamicClient _dynamicClient;

        public RagController(DynamicClient dynamicClient)
        {
            _dynamicClient = dynamicClient;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadPdf(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return Json(new { success = false, message = "A valid PDF file artifact must be selected." });
            }

            try
            {
                using var httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7154/") };
                using var content = new MultipartFormDataContent();
                using var fileStream = file.OpenReadStream();
                using var streamContent = new StreamContent(fileStream);

                content.Add(streamContent, "file", file.FileName);

                using var response = await httpClient.PostAsync("api/rag/upload", content);
                response.EnsureSuccessStatusCode();

                string backendMessage = await response.Content.ReadAsStringAsync();
                return Json(new { success = true, message = backendMessage });
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

        [HttpGet]
        public async Task<IActionResult> Query(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                return Json(new { success = false, message = "Prompt cannot be empty." });
            }

            try
            {
                string url = $"api/rag/query?prompt={Uri.EscapeDataString(prompt)}";
                string aiResponse = await _dynamicClient.SendPostReturnStringAsync(url, prompt);
                return Json(new { success = true, response = aiResponse });
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
    }
}