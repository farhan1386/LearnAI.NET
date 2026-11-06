using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;

namespace LearnAI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CodeReviewController : ControllerBase
    {
        private readonly Microsoft.SemanticKernel.Kernel _kernel;

        public CodeReviewController(Microsoft.SemanticKernel.Kernel kernel)
        {
            _kernel = kernel;
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzeCode([FromBody] CodeReviewRequest request)
        {
            try
            {
                string executionPath = AppContext.BaseDirectory;
                string promptsDirectory = Path.Combine(executionPath, "Prompts");

                if (!Directory.Exists(Path.Combine(promptsDirectory, "CodeReview")))
                {
                    return BadRequest("Prompt directory configuration 'Prompts/CodeReview' could not be found in output binary path.");
                }

                var codeReviewPlugin = _kernel.CreatePluginFromPromptDirectory(promptsDirectory, "CodeReview");

                var arguments = new KernelArguments
                {
                    { "input", request.CodeSnippet },
                    { "language", request.Language }
                };

                var result = await _kernel.InvokeAsync(codeReviewPlugin["Analyze"], arguments);

                return Ok(new { assessment = result.ToString() });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal kernel execution error: {ex.Message}");
            }
        }
    }

    public class CodeReviewRequest
    {
        public string CodeSnippet { get; set; } = string.Empty;
        public string Language { get; set; } = "C#";
    }
}