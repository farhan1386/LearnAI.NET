using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LearnAI.Tools.Interfaces;
using LearnAI.Tools.Models;

namespace LearnAI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ToolsController : ControllerBase
    {
        private readonly IToolExecutionEngine _toolEngine;

        public ToolsController(IToolExecutionEngine toolEngine)
        {
            _toolEngine = toolEngine ?? throw new ArgumentNullException(nameof(toolEngine));
        }

        [HttpPost("execute")]
        public async Task<IActionResult> ExecuteTool([FromBody] ToolExecutionRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Prompt))
            {
                return BadRequest(new { Success = false, Output = string.Empty, Error = "User prompt cannot be empty." });
            }

            try
            {
                var vendorOptions = new
                {
                    ToolChoice = request.SelectionMode.ToString().ToLowerInvariant(),
                    Temperature = 0.0
                };

                var result = await _toolEngine.ProcessExecutionAsync(
                    request.Prompt,
                    vendorOptions,
                    request.SelectionMode,
                    request.TargetToolName
                );

                return Ok(new { Success = true, Output = result, Error = (string?)null });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Output = string.Empty, Error = ex.Message });
            }
        }
    }

    public class ToolExecutionRequest
    {
        public string Prompt { get; set; } = string.Empty;
        public ToolSelectionMode SelectionMode { get; set; } = ToolSelectionMode.Auto;
        public string? TargetToolName { get; set; }
    }
}
