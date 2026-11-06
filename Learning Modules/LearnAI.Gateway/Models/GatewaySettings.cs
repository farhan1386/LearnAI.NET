namespace LearnAI.Gateway.Models
{
    public class GatewaySettings
    {
        public OpenAISettings OpenAI { get; set; } = new();
        public AzureOpenAISettings AzureOpenAI { get; set; } = new();
        public ClaudeSettings Claude { get; set; } = new();
        public GeminiSettings Gemini { get; set; } = new();
        public OllamaSettings Ollama { get; set; } = new();
    }

    public class OpenAISettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string DefaultModel { get; set; } = "gpt-4o-mini";
    }

    public class AzureOpenAISettings
    {
        public string Endpoint { get; set; } = "https://azure.com";
        public string DefaultDeployment { get; set; } = "gpt-4o";
    }

    public class ClaudeSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string DefaultModel { get; set; } = "claude-3-5-sonnet";
    }

    public class GeminiSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string DefaultModel { get; set; } = "gemini-2.5-flash";
    }

    public class OllamaSettings
    {
        public string Endpoint { get; set; } = "http://localhost:11434";
        public string DefaultModel { get; set; } = "phi4";
    }
}