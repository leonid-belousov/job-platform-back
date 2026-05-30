using System.Net.Http.Json;
using System.Text.Json;

namespace JobPlatform.API.Tests.Infrastructure;

public static class TestJson
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static async Task<T> ReadAsAsync<T>(this HttpContent content)
    {
        var value = await content.ReadFromJsonAsync<T>(Options);
        return value ?? throw new InvalidOperationException($"Response body could not be deserialized as {typeof(T).Name}.");
    }
}
