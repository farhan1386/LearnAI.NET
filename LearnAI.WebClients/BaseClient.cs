using System.Net.Http.Json;

namespace LearnAI.WebClients
{
    public class BaseClient
    {
        protected readonly HttpClient HttpClient;

        public BaseClient(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        protected async Task<T> GetAsync<T>(string requestUri, CancellationToken cancellationToken = default) where T : new()
        {
            var response = await HttpClient.GetAsync(requestUri, cancellationToken);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
            return result ?? new T();
        }

        protected async Task<TResponse> PostAsync<TRequest, TResponse>(string requestUri, TRequest requestData, CancellationToken cancellationToken = default) where TResponse : new()
        {
            var response = await HttpClient.PostAsJsonAsync(requestUri, requestData, cancellationToken);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);
            return result ?? new TResponse();
        }

        protected async Task<string> PostReturnStringAsync<TRequest>(string requestUri, TRequest requestData, CancellationToken cancellationToken = default)
        {
            var response = await HttpClient.PostAsJsonAsync(requestUri, requestData, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(cancellationToken) ?? string.Empty;
        }
    }
}
