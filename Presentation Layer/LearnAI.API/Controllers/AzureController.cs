using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;

namespace LearnAI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AzureController : ControllerBase
    {
        private readonly IChatClient _chatClient;

        public AzureController(IChatClient chatClient)
        {
            _chatClient = chatClient ?? throw new ArgumentNullException(nameof(chatClient));
        }

        [HttpPost("chat")]
        public async Task<IActionResult> ProcessChat([FromBody] AzureChatRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
            {
                return BadRequest("Prompt parameter is required.");
            }

            try
            {
                var messages = new List<ChatMessage>();

                if (!string.IsNullOrWhiteSpace(request.SystemMessage))
                {
                    messages.Add(new ChatMessage(ChatRole.System, request.SystemMessage));
                }

                messages.Add(new ChatMessage(ChatRole.User, request.Prompt));

                ChatResponse response = await _chatClient.GetResponseAsync(messages);
                return Ok(new { text = response.Text });
            }
            catch (OperationCanceledException ex)
            {
                return StatusCode(403, new { error = "Security Block", details = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Infrastructure Failure", details = ex.Message });
            }
        }

        [HttpPost("stream")]
        public async IAsyncEnumerable<string> ProcessStream([FromBody] AzureChatRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
            {
                yield break;
            }

            var messages = new List<ChatMessage>();

            if (!string.IsNullOrWhiteSpace(request.SystemMessage))
            {
                messages.Add(new ChatMessage(ChatRole.System, request.SystemMessage));
            }

            messages.Add(new ChatMessage(ChatRole.User, request.Prompt));

            IAsyncEnumerable<ChatResponseUpdate> streamUpdates;

            try
            {
                streamUpdates = _chatClient.GetStreamingResponseAsync(messages);
            }
            catch (Exception)
            {
                yield break;
            }

            await foreach (var update in streamUpdates)
            {
                if (!string.IsNullOrEmpty(update.Text))
                {
                    yield return update.Text;
                }
            }
        }
    }

    public class AzureChatRequestDto
    {
        public string Prompt { get; set; } = string.Empty;
        public string? SystemMessage { get; set; }
    }
}
