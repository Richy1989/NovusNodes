using System.Text.Json;

namespace NovusNodoCore.Extensions
{
    internal class JsonExtensions
    {
        public static async Task<string> SerializeObjectToJsonAsync<T>(T obj)
        {
            // Create a memory stream to hold the serialized JSON
            await using var memoryStream = new MemoryStream();

            // Serialize the object to the memory stream
            await JsonSerializer.SerializeAsync(memoryStream, obj);

            // Reset the memory stream position to the beginning
            memoryStream.Position = 0;

            // Read the memory stream into a string
            using var reader = new StreamReader(memoryStream);
            return await reader.ReadToEndAsync();
        }
    }
}
