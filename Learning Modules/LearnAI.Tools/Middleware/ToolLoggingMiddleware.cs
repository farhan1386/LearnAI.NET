using LearnAI.Tools.Interfaces;
using LearnAI.Tools.Models;
using System.Diagnostics;

namespace LearnAI.Tools.Middleware
{
    public class ToolLoggingMiddleware : IToolMiddleware
    {
        public async Task<string> InvokeAsync(ToolExecutionContext context, Func<Task<string>> next)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var result = await next();
                stopwatch.Stop();
                context.Metadata["ExecutionTimeMs"] = stopwatch.ElapsedMilliseconds;
                context.Metadata["IsSuccess"] = true;
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                context.Metadata["ExecutionTimeMs"] = stopwatch.ElapsedMilliseconds;
                context.Metadata["IsSuccess"] = false;
                context.Metadata["ErrorReason"] = ex.Message;
                throw;
            }
        }
    }
}