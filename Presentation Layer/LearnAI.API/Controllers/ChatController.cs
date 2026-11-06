using LearnAI.Chat.Models;
using LearnAI.Chat.Services;
using Microsoft.AspNetCore.Mvc;

namespace LearnAI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ChatService _chatService;

        public ChatController(ChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string question, [FromQuery] string? sessionId, [FromQuery] string model = "phi4", [FromQuery] float temperature = 0.7f, [FromQuery] int maxTokens = 1000)
        {
            if (string.IsNullOrWhiteSpace(question))
            {
                return BadRequest("The 'question' query parameter is required.");
            }

            var request = new ChatRequest
            {
                Message = question,
                AIModel = model,
                SessionId = sessionId,
                Temperature = temperature,
                MaximumTokens = maxTokens
            };

            var result = await _chatService.AskAsync(request);

            if (!result.IsSuccess)
            {
                return BadRequest(result.ErrorMessage);
            }

            return Ok(result);
        }

        [HttpGet("stream")]
        public async Task Stream([FromQuery] string question, [FromQuery] string? sessionId, [FromQuery] string model = "phi4", [FromQuery] float temperature = 0.7f, [FromQuery] int maxTokens = 1000, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(question))
            {
                Response.StatusCode = 400;
                await Response.WriteAsync("The 'question' query parameter is required.", cancellationToken);
                return;
            }

            Response.ContentType = "text/event-stream";
            Response.Headers.Append("Cache-Control", "no-cache");
            Response.Headers.Append("Connection", "keep-alive");

            var request = new ChatRequest
            {
                Message = question,
                AIModel = model,
                SessionId = sessionId,
                Temperature = temperature,
                MaximumTokens = maxTokens
            };

            try
            {
                await foreach (var chunk in _chatService.StreamAsync(request, cancellationToken).WithCancellation(cancellationToken))
                {
                    await Response.WriteAsync($"data: {chunk}\n\n", cancellationToken);
                    await Response.Body.FlushAsync(cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}