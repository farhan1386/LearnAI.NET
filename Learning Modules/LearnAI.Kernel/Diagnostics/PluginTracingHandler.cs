using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LearnAI.Kernel.Diagnostics
{
    public class PluginTracingHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var response = await base.SendAsync(request, cancellationToken);
                stopwatch.Stop();

                Console.WriteLine($"[SK-Trace] Outgoing request to {request.RequestUri?.Host} completed in {stopwatch.ElapsedMilliseconds}ms with status {response.StatusCode}");
                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Console.WriteLine($"[SK-Trace] Outgoing request to {request.RequestUri?.Host} failed after {stopwatch.ElapsedMilliseconds}ms. Error: {ex.Message}");
                throw;
            }
        }
    }
}