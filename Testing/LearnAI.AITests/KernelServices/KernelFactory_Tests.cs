using LearnAI.Kernel.Services;
using Microsoft.Extensions.Configuration;

namespace LearnAI.AITests.KernelServices
{
    public class KernelFactory_Tests
    {
        [Fact]
        public void CreateKernel_WithValidConfiguration_ReturnsConfiguredKernel()
        {
            var inMemorySettings = new Dictionary<string, string>
            {
                {"AIProviders:OpenAI:ApiKey", "test-api-key"},
                {"AIProviders:OpenAI:ModelId", "gpt-4o"}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var factory = new KernelFactory(configuration);

            var kernel = factory.CreateKernel();

            Assert.NotNull(kernel);
            Assert.NotNull(kernel.Plugins);
            Assert.Contains(kernel.Plugins, p => p.Name == "TimePlugin");
            Assert.Contains(kernel.Plugins, p => p.Name == "MathPlugin");
        }

        [Fact]
        public void CreateKernel_WithMissingApiKey_ReturnsKernelWithoutOpenAiServices()
        {
            var inMemorySettings = new Dictionary<string, string>
            {
                {"AIProviders:OpenAI:ApiKey", ""},
                {"AIProviders:OpenAI:ModelId", "gpt-4o"}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var factory = new KernelFactory(configuration);

            var kernel = factory.CreateKernel();

            Assert.NotNull(kernel);
            Assert.Contains(kernel.Plugins, p => p.Name == "TimePlugin");
            Assert.Contains(kernel.Plugins, p => p.Name == "MathPlugin");
        }
    }
}
