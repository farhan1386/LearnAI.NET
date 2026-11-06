using LearnAI.Tools.Interfaces;
using LearnAI.Tools.Models;
using System.Reflection;
using System.Text.Json;
using System.Runtime.CompilerServices;

namespace LearnAI.Tools.Services
{
    public class StructuredOutputService
    {
        private readonly IJsonSchemaBuilder _schemaBuilder;

        public StructuredOutputService(IJsonSchemaBuilder schemaBuilder)
        {
            _schemaBuilder = schemaBuilder ?? throw new ArgumentNullException(nameof(schemaBuilder));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void DummyMethodExtractor<T>(T payloadStructure) { }

        public string GenerateSchemaForType<T>()
        {
            var propertiesMethod = typeof(StructuredOutputService)
                .GetMethod(nameof(DummyMethodExtractor), BindingFlags.NonPublic | BindingFlags.Instance);

            var genericMethod = propertiesMethod?.MakeGenericMethod(typeof(T));
            if (genericMethod == null) throw new InvalidOperationException("Failed to extract type metadata.");

            return _schemaBuilder.BuildSchema(genericMethod);
        }

        public void ConfigureStrictJsonOutput(object vendorChatOptions, string schemaJson, string rootName = "json_response")
        {
            if (vendorChatOptions == null) throw new ArgumentNullException(nameof(vendorChatOptions));

            var optionsType = vendorChatOptions.GetType();
            var responseFormatProp = optionsType.GetProperty("ResponseFormat");
            if (responseFormatProp == null) return;

            try
            {
                using var schemaDoc = JsonDocument.Parse(schemaJson);
                var targetFormat = new
                {
                    type = "json_object",
                    json_schema = new
                    {
                        name = rootName,
                        strict = true,
                        schema = schemaDoc.RootElement
                    }
                };

                var serializedFormat = JsonSerializer.Serialize(targetFormat);
                var stronglyTypedFormat = JsonSerializer.Deserialize(serializedFormat, responseFormatProp.PropertyType);
                responseFormatProp.SetValue(vendorChatOptions, stronglyTypedFormat);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to bind structural JSON parameters.", ex);
            }
        }

        public ToolExecutionResult<T> DeserializeWithRetry<T>(string rawJson, Func<string, string>? retryPromptHandler = null, int maxAttempts = 3) where T : class
        {
            var result = new ToolExecutionResult<T> { RawPayload = rawJson, AttemptsTaken = 0 };
            var currentPayload = CleanMarkdownWrappers(rawJson);

            while (result.AttemptsTaken < maxAttempts)
            {
                result.AttemptsTaken++;
                try
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, AllowTrailingCommas = true };
                    result.Data = JsonSerializer.Deserialize<T>(currentPayload, options);
                    if (result.Data != null)
                    {
                        result.IsSuccess = true;
                        return result;
                    }
                }
                catch (JsonException ex)
                {
                    result.ErrorMessage = ex.Message;
                    if (retryPromptHandler == null || result.AttemptsTaken >= maxAttempts) break;

                    try
                    {
                        currentPayload = CleanMarkdownWrappers(retryPromptHandler(ex.Message));
                    }
                    catch
                    {
                        break;
                    }
                }
            }

            result.IsSuccess = false;
            return result;
        }

        private static string CleanMarkdownWrappers(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var trimmed = input.Trim();

            if (trimmed.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
            {
                trimmed = trimmed[7..];
                if (trimmed.EndsWith("```")) trimmed = trimmed[..^3];
            }
            else if (trimmed.StartsWith("```"))
            {
                trimmed = trimmed[3..];
                if (trimmed.EndsWith("```")) trimmed = trimmed[..^3];
            }

            return trimmed.Trim();
        }
    }
}
