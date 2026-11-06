using System.ComponentModel.DataAnnotations;
using LearnAI.Tools.Models;

namespace LearnAI.Web.Models
{
    public class ToolsViewModel
    {
        [Required(ErrorMessage = "Please provide an intent prompt calculation task request.")]
        public string Prompt { get; set; } = string.Empty;

        public ToolSelectionMode SelectionMode { get; set; } = ToolSelectionMode.Auto;

        public string? TargetToolName { get; set; }

        public string? ExecutionResult { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
