using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.Client;
using Newtonsoft.Json;

// ReSharper disable UnusedParameterInPartialMethod

namespace JsonApiDotNetCoreExampleClient;

[UsedImplicitly(ImplicitUseTargetFlags.Itself)]
public partial class ExampleApiClient : JsonApiClient
{
    partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
    {
        SetSerializerSettingsForJsonApi(settings);

        // Optional: Makes the JSON easier to read when logged.
        settings.Formatting = Formatting.Indented;
    }

    // Optional: Log outgoing request to the console.
    partial void PrepareRequest(HttpClient client, HttpRequestMessage request, string url)
    {
        using var _ = new UsingConsoleColor(ConsoleColor.Green);

        Console.WriteLine($"--> {request}");
        string? requestBody = request.Content?.ReadAsStringAsync().GetAwaiter().GetResult();

        if (!string.IsNullOrEmpty(requestBody))
        {
            Console.WriteLine();
            Console.WriteLine(requestBody);
        }
    }

    // Optional: Log incoming response to the console.
    partial void ProcessResponse(HttpClient client, HttpResponseMessage response)
    {
        using var _ = new UsingConsoleColor(ConsoleColor.Cyan);

        Console.WriteLine($"<-- {response}");
        string responseBody = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

        if (!string.IsNullOrEmpty(responseBody))
        {
            Console.WriteLine();
            Console.WriteLine(responseBody);
        }
    }

    private sealed class UsingConsoleColor : IDisposable
    {
        public UsingConsoleColor(ConsoleColor foregroundColor)
        {
            Console.ForegroundColor = foregroundColor;
        }

        public void Dispose()
        {
            Console.ResetColor();
        }
    }
}
