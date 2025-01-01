using System.Text.Json;

namespace NovusNodo.Helper
{
    public class JsonHelper
    {
        public static (string Id, string Port) LinkExtractIdAndPort(string json)
        {
            var jsonObject = JsonDocument.Parse(json).RootElement;

            string id = jsonObject.GetProperty("id").GetString();
            string port = jsonObject.GetProperty("port").GetString();

            return (id, port);
        }
    }
}
