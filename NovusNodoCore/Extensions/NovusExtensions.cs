using System.Reflection;
using System.Text.Json;

namespace NovusNodoCore.Extensions
{
    internal static class NovusExtensions
    {
        /// <summary>
        /// Serializes an object to a JSON string asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="obj">The object to serialize.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the JSON string representation of the object.</returns>
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

        public static T GetPropertyValue<T>(this Type t, string name)
        {
            if (t == null)
                return default(T);

            BindingFlags flags = BindingFlags.Static | BindingFlags.Public;

            PropertyInfo info = t.GetProperty(name, flags);

            if (info == null)
            {
                // See if we have a field;
                FieldInfo finfo = t.GetField(name, flags);
                if (finfo == null)
                    return default(T);

                return (T)finfo.GetValue(null);
            }

            return (T)info.GetValue(null, null);
        }
    }
}
