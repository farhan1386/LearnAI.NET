using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LearnAI.MCP.Services;
using LearnAI.MCP.Models;

namespace LearnAI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MCPController : ControllerBase
    {
        private readonly McpService _orchestrator;

        public MCPController(McpService orchestrator)
        {
            _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
        }

        [HttpGet("tools")]
        public async Task<ActionResult<IEnumerable<McpTool>>> GetAvailableTools(CancellationToken cancellationToken)
        {
            var tools = await _orchestrator.AggregateAvailableToolsAsync(cancellationToken);
            return Ok(tools);
        }

        [HttpPost("tools/execute")]
        public async Task<IActionResult> ExecuteTool(
            [FromQuery] string serverName,
            [FromQuery] string toolName,
            [FromBody] Dictionary<string, object> arguments,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(serverName) || string.IsNullOrEmpty(toolName))
            {
                return BadRequest("Server name and tool name are required.");
            }

            try
            {
                var jsonResult = await _orchestrator.ProcessToolTokenPipelineAsync(serverName, toolName, arguments, cancellationToken);
                return Content(jsonResult, "application/json");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}