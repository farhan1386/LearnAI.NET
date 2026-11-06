using LearnAI.Tools.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.Json;

namespace LearnAI.AITests.ToolsServices
{
    public class JsonSchemaBuilder_Tests
    {
        private readonly JsonSchemaBuilder _builder;

        public JsonSchemaBuilder_Tests()
        {
            _builder = new JsonSchemaBuilder();
        }

        private class MockTestTools
        {
            public void SimpleMethod(
                [Description("Target identity city name context mapping")] string location,
                int count)
            {
            }

            public void ComplexMethod(
                string[] values,
                DayOfWeek mode = DayOfWeek.Monday)
            {
            }
        }

        [Fact]
        public void BuildSchema_ShouldExtractValidPrimitiveTypesAndDescriptions()
        {
            var methodInfo = typeof(MockTestTools).GetMethod(nameof(MockTestTools.SimpleMethod));
            Assert.NotNull(methodInfo);

            var jsonResult = _builder.BuildSchema(methodInfo);
            Assert.False(string.IsNullOrWhiteSpace(jsonResult));

            using var doc = JsonDocument.Parse(jsonResult);
            var root = doc.RootElement;

            Assert.Equal("object", root.GetProperty("type").GetString());

            var properties = root.GetProperty("properties");
            var locationProp = properties.GetProperty("location");

            Assert.Equal("string", locationProp.GetProperty("type").GetString());
            Assert.Equal("Target identity city name context mapping", locationProp.GetProperty("description").GetString());

            var countProp = properties.GetProperty("count");
            Assert.Equal("integer", countProp.GetProperty("type").GetString());
        }

        [Fact]
        public void BuildSchema_ShouldEnforceRequiredParametersExcludingDefaults()
        {
            var methodInfo = typeof(MockTestTools).GetMethod(nameof(MockTestTools.SimpleMethod));
            Assert.NotNull(methodInfo);

            var jsonResult = _builder.BuildSchema(methodInfo);
            using var doc = JsonDocument.Parse(jsonResult);
            var root = doc.RootElement;

            Assert.True(root.TryGetProperty("required", out var requiredArray));
            Assert.Contains("location", requiredArray.GetRawText());
            Assert.Contains("count", requiredArray.GetRawText());
        }

        [Fact]
        public void BuildSchema_ShouldMapArraysAndEnumStructuresCorrectly()
        {
            var methodInfo = typeof(MockTestTools).GetMethod(nameof(MockTestTools.ComplexMethod));
            Assert.NotNull(methodInfo);

            var jsonResult = _builder.BuildSchema(methodInfo);
            using var doc = JsonDocument.Parse(jsonResult);
            var root = doc.RootElement;

            var properties = root.GetProperty("properties");

            var valuesProp = properties.GetProperty("values");
            Assert.Equal("array", valuesProp.GetProperty("type").GetString());

            var modeProp = properties.GetProperty("mode");
            Assert.Equal("string", modeProp.GetProperty("type").GetString());
            Assert.True(modeProp.TryGetProperty("enum", out var enumValues));
            Assert.Contains("Monday", enumValues.GetRawText());

            Assert.True(root.TryGetProperty("required", out var requiredArray));
            Assert.Contains("values", requiredArray.GetRawText());
            Assert.DoesNotContain("mode", requiredArray.GetRawText());
        }
    }
}
