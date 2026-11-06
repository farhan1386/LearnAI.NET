using System;

namespace LearnAI.MCP.Services
{
    public class McpTracingService
    {
        public void LogTrace(string message)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"[TRACE] [{DateTime.UtcNow:HH:mm:ss.fff}] {message}");
            Console.ResetColor();
        }

        public void LogError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERROR] [{DateTime.UtcNow:HH:mm:ss.fff}] {message}");
            Console.ResetColor();
        }
    }
}
