using System.Text.Json;

namespace NovusNodoPluginLibrary.Helper
{
    /// <summary>
    /// Provides methods to extract variables from a JSON string.
    /// </summary>
    public class JsonVariableExtractor
    {
        /// <summary>
        /// Asynchronously extracts variables from a JSON string.
        /// </summary>
        /// <param name="jsonString">The JSON string to extract variables from.</param>
        /// <returns>A dictionary containing the extracted variables.</returns>
        public static async Task<Dictionary<string, object>> ExtractVariablesAsync(string jsonString)
        {
            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonString));
            try
            {
                // Parse the JSON stream asynchronously into a JsonDocument
                using JsonDocument doc = await JsonDocument.ParseAsync(stream);
                JsonElement root = doc.RootElement;

                // Convert the JSON structure to a dictionary
                return ParseJsonElement(root);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Invalid JSON: {ex.Message}");
                return new Dictionary<string, object>();
            }
        }

        /// <summary>
        /// Parses a JsonElement into a dictionary.
        /// </summary>
        /// <param name="element">The JsonElement to parse.</param>
        /// <returns>A dictionary containing the parsed variables.</returns>
        private static Dictionary<string, object> ParseJsonElement(JsonElement element)
        {
            Dictionary<string, object> variables = [];

            foreach (var property in element.EnumerateObject())
            {
                variables[property.Name] = ExtractValue(property.Value);
            }

            return variables;
        }

        /// <summary>
        /// Extracts the value from a JsonElement.
        /// </summary>
        /// <param name="element">The JsonElement to extract the value from.</param>
        /// <returns>The extracted value.</returns>
        private static object ExtractValue(JsonElement element)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.TryGetInt64(out long l) ? l : element.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Array => ExtractArray(element),
                JsonValueKind.Object => ParseJsonElement(element),
                JsonValueKind.Null => null,
                JsonValueKind.Undefined => element.GetString(),
                _ => throw new NotSupportedException($"Unsupported JSON value kind: {element.ValueKind}")
            };
#pragma warning restore CS8603 // Possible null reference return.
        }

        /// <summary>
        /// Extracts the values from a JSON array element.
        /// </summary>
        /// <param name="arrayElement">The JSON array element to extract values from.</param>
        /// <returns>A list containing the extracted values.</returns>
        private static object ExtractArray(JsonElement arrayElement)
        {
            List<object> array = [];
            foreach (var item in arrayElement.EnumerateArray())
            {
                array.Add(ExtractValue(item));
            }
            return array;
        }
    }
}