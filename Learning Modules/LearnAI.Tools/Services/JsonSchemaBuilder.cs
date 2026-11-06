using LearnAI.Tools.Interfaces;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;

namespace LearnAI.Tools.Services
{
    public class JsonSchemaBuilder : IJsonSchemaBuilder
    {
        public string BuildSchema(MethodInfo methodInfo)
        {
            if (methodInfo == null) throw new ArgumentNullException(nameof(methodInfo));

            var parameters = methodInfo.GetParameters();
            var properties = new Dictionary<string, object>();
            var requiredList = new List<string>();

            foreach (var param in parameters)
            {
                if (string.IsNullOrEmpty(param.Name)) continue;

                var paramSchema = new Dictionary<string, object>
                {
                    { "type", MapTypeToJsonSchema(param.ParameterType) }
                };

                var descriptionAttr = param.GetCustomAttribute<DescriptionAttribute>();
                if (descriptionAttr != null)
                {
                    paramSchema.Add("description", descriptionAttr.Description);
                }

                if (param.ParameterType.IsEnum)
                {
                    paramSchema.Add("enum", Enum.GetNames(param.ParameterType));
                }

                properties.Add(param.Name, paramSchema);

                if (!param.HasDefaultValue)
                {
                    requiredList.Add(param.Name);
                }
            }

            var schemaRoot = new Dictionary<string, object>
            {
                { "type", "object" },
                { "properties", properties }
            };

            if (requiredList.Any())
            {
                schemaRoot.Add("required", requiredList);
            }

            return JsonSerializer.Serialize(schemaRoot, new JsonSerializerOptions { WriteIndented = true });
        }

        private static string MapTypeToJsonSchema(Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            if (underlyingType == typeof(string) || underlyingType == typeof(char) || underlyingType == typeof(Guid))
                return "string";

            if (underlyingType == typeof(int) || underlyingType == typeof(long) || underlyingType == typeof(short) || underlyingType == typeof(byte))
                return "integer";

            if (underlyingType == typeof(double) || underlyingType == typeof(float) || underlyingType == typeof(decimal))
                return "number";

            if (underlyingType == typeof(bool))
                return "boolean";

            if (underlyingType.IsArray || (underlyingType.IsGenericType && typeof(System.Collections.IEnumerable).IsAssignableFrom(underlyingType)))
                return "array";

            if (underlyingType.IsEnum)
                return "string";

            return "object";
        }
    }
}