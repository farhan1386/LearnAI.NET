using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using LearnAI.Tools.Interfaces;
using LearnAI.Tools.Models;

namespace LearnAI.Tools.Middleware
{
    public class ToolSandboxMiddleware : IToolMiddleware
    {
        private readonly string _allowedRootDirectory;

        public ToolSandboxMiddleware(string? allowedRootDirectory = null)
        {
            _allowedRootDirectory = allowedRootDirectory ?? AppContext.BaseDirectory;
        }

        public async Task<string> InvokeAsync(ToolExecutionContext context, Func<Task<string>> next)
        {
            if (context.Tool.Definition.Name.StartsWith("File", StringComparison.OrdinalIgnoreCase) &&
                context.RawArgumentsJson.Contains(":\""))
            {
                try
                {
                    using var doc = JsonDocument.Parse(context.RawArgumentsJson);
                    foreach (var property in doc.RootElement.EnumerateObject())
                    {
                        if (property.Name.Contains("path", StringComparison.OrdinalIgnoreCase) ||
                            property.Name.Contains("file", StringComparison.OrdinalIgnoreCase))
                        {
                            var targetPath = property.Value.GetString();
                            if (!string.IsNullOrEmpty(targetPath))
                            {
                                var fullPath = Path.GetFullPath(targetPath);
                                var allowedRoot = Path.GetFullPath(_allowedRootDirectory);

                                if (!fullPath.StartsWith(allowedRoot, StringComparison.OrdinalIgnoreCase))
                                {
                                    throw new UnauthorizedAccessException("Execution blocked: Access path out of sandbox boundaries.");
                                }
                            }
                        }
                    }
                }
                catch (JsonException)
                {
                    throw new InvalidOperationException("Execution blocked: Invalid argument payload formatting.");
                }
            }

            return await next();
        }
    }
}