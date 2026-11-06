using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LearnAI.WebClients
{
    public class DynamicClient : BaseClient
    {
        public DynamicClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<TResponse> SendPostAsync<TRequest, TResponse>(string requestUri, TRequest requestData, CancellationToken cancellationToken = default) where TResponse : new()
        {
            return await PostAsync<TRequest, TResponse>(requestUri, requestData, cancellationToken);
        }

        public async Task<string> SendPostReturnStringAsync<TRequest>(string requestUri, TRequest requestData, CancellationToken cancellationToken = default)
        {
            return await PostReturnStringAsync(requestUri, requestData, cancellationToken);
        }

        public async Task<T> SendGetAsync<T>(string requestUri, CancellationToken cancellationToken = default) where T : new()
        {
            return await GetAsync<T>(requestUri, cancellationToken);
        }

        public async Task<HttpResponseMessage> SendGetStreamAsync(string requestUri, CancellationToken cancellationToken = default)
        {
            return await HttpClient.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        }

        public async Task<JsonElement> ExecuteMcpAsync(string method, object? arguments, CancellationToken cancellationToken = default)
        {
            var payload = new { Method = method, Arguments = arguments };
            return await PostAsync<object, JsonElement>("api/MCP/execute", payload, cancellationToken);
        }
    }
}