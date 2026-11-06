using System;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.AI.Projects;
using Microsoft.Extensions.Logging;

namespace LearnAI.Azure.Foundry
{
    public class AzureAIFoundrySettings
    {
        private readonly AIProjectClient _projectClient;
        private readonly ILogger<AzureAIFoundrySettings> _logger;

        public AzureAIFoundrySettings(string projectEndpoint, ILogger<AzureAIFoundrySettings> logger)
        {
            if (string.IsNullOrWhiteSpace(projectEndpoint))
                throw new ArgumentNullException(nameof(projectEndpoint));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _projectClient = new AIProjectClient(new Uri(projectEndpoint), new DefaultAzureCredential());
        }

        public async Task EnumerateConnectedAssetsAsync()
        {
            try
            {
                _logger.LogInformation("Discovering Azure AI Foundry workspace connections...");

                var connections = _projectClient.Connections.GetConnectionsAsync();

                await foreach (var connection in connections)
                {
                    _logger.LogInformation("Found Connected Asset: {Name} | Type: {Type} | Target: {Target}",
                        connection.Name,
                        connection.Type,
                        connection.Target);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resolve Azure AI Foundry cloud configurations.");
                throw;
            }
        }

        public AIProjectClient GetProjectClient() => _projectClient;
    }
}