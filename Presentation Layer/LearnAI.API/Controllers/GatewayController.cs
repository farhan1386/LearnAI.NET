using LearnAI.Gateway.Interfaces;
using LearnAI.Gateway.Models;
using Microsoft.AspNetCore.Mvc;

namespace LearnAI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GatewayController : ControllerBase
    {
        private readonly IGatewayService _gatewayService;

        public GatewayController(IGatewayService gatewayService)
        {
            _gatewayService = gatewayService ?? throw new ArgumentNullException(nameof(gatewayService));
        }

        [HttpPost("switch")]
        public async Task<IActionResult> SwitchAndExecute([FromBody] GatewayRequest request)
        {
            if (request == null)
            {
                return BadRequest("A valid gateway execution request body must be supplied.");
            }

            try
            {
                var responseText = await _gatewayService.ExecutePromptAsync(request);
                return Ok(new { Response = responseText });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("compare")]
        public async Task<IActionResult> CompareBackends([FromQuery] string prompt, [FromQuery] string? modelName = null)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                return BadRequest("The evaluation prompt parameter cannot be empty.");
            }

            try
            {
                var comparisonReport = await _gatewayService.CompareProvidersAsync(prompt, modelName);
                return Ok(comparisonReport);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}