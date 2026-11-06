using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LearnAI.MCP.Models;

namespace LearnAI.MCP.Prompts
{
    public class PromptRegistry
    {
        private readonly ConcurrentDictionary<string, (McpPrompt Metadata, Func<Dictionary<string, string>, string> TemplateEngine)> _prompts = new();

        public void RegisterPrompt(McpPrompt prompt, Func<Dictionary<string, string>, string> templateEngine)
        {
            if (prompt == null) throw new ArgumentNullException(nameof(prompt));
            if (templateEngine == null) throw new ArgumentNullException(nameof(templateEngine));

            _prompts[prompt.Name] = (prompt, templateEngine);
        }

        public Task<IEnumerable<McpPrompt>> GetPromptsAsync(CancellationToken cancellationToken = default)
        {
            var metadataList = new List<McpPrompt>();
            foreach (var item in _prompts.Values)
            {
                metadataList.Add(item.Metadata);
            }
            return Task.FromResult<IEnumerable<McpPrompt>>(metadataList);
        }

        public Task<string> RenderPromptAsync(string name, Dictionary<string, string> arguments, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Prompt name cannot be null or empty.", nameof(name));

            if (!_prompts.TryGetValue(name, out var mapping))
            {
                throw new KeyNotFoundException($"Prompt with name '{name}' was not found in the registry.");
            }

            foreach (var arg in mapping.Metadata.Arguments)
            {
                if (arg.Required && (arguments == null || !arguments.ContainsKey(arg.Name) || string.IsNullOrEmpty(arguments[arg.Name])))
                {
                    throw new ArgumentException($"Missing required prompt argument: '{arg.Name}'", nameof(arguments));
                }
            }

            string renderedText = mapping.TemplateEngine(arguments ?? new Dictionary<string, string>());
            return Task.FromResult(renderedText);
        }
    }
}