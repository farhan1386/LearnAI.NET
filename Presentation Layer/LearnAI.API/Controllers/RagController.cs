using LearnAI.RAG.Interfaces;
using LearnAI.RAG.Services;
using Microsoft.AspNetCore.Mvc;

namespace LearnAI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RagController : ControllerBase
    {
        private readonly PdfExtractorService _pdfExtractor;
        private readonly IRagOrchestrator _ragOrchestrator;

        public RagController(PdfExtractorService pdfExtractor, IRagOrchestrator ragOrchestrator)
        {
            _pdfExtractor = pdfExtractor ?? throw new ArgumentNullException(nameof(pdfExtractor));
            _ragOrchestrator = ragOrchestrator ?? throw new ArgumentNullException(nameof(ragOrchestrator));
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadPdf(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("A valid PDF file artifact must be uploaded.");
            }

            if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Only PDF document parsing is supported.");
            }

            try
            {
                using (var stream = file.OpenReadStream())
                {
                    string extractedText = _pdfExtractor.ExtractTextFromStream(stream);
                    await _ragOrchestrator.IngestDocumentAsync(extractedText, file.FileName);
                }

                return Ok(new { Message = $"Successfully ingested and vectorized: '{file.FileName}'." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Failed to parse document: {ex.Message}" });
            }
        }

        [HttpGet("query")]
        public async Task<IActionResult> QueryDocument([FromQuery] string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                return BadRequest("The query prompt cannot be empty.");
            }

            try
            {
                string executionResponse = await _ragOrchestrator.QueryAsync(prompt);
                return Ok(new { Response = executionResponse });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Retrieval execution failure: {ex.Message}" });
            }
        }
    }
}