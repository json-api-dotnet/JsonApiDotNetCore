using JetBrains.Annotations;

namespace OpenApiNSwagClientExample;

/// <summary>
/// Writes incoming and outgoing HTTP messages to the console.
/// </summary>
[UsedImplicitly]
internal sealed class ColoredConsoleLogDelegatingHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await LogRequestAsync(request, cancellationToken);

        HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

        await LogResponseAsync(response, cancellationToken);

        return response;
    }

    private static async Task LogRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using var _ = new ConsoleColorScope(ConsoleColor.Green);

        Console.WriteLine($"--> {request}");
        string? requestBody = request.Content != null ? await request.Content.ReadAsStringAsync(cancellationToken) : null;

        if (!string.IsNullOrEmpty(requestBody))
        {
            Console.WriteLine();
            Console.WriteLine(requestBody);
        }
    }

    private static async Task LogResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        using var _ = new ConsoleColorScope(ConsoleColor.Cyan);

        Console.WriteLine($"<-- {response}");
        string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!string.IsNullOrEmpty(responseBody))
        {
            Console.WriteLine();
            Console.WriteLine(responseBody);
        }
    }

    private sealed class ConsoleColorScope : IDisposable
    {
        public ConsoleColorScope(ConsoleColor foregroundColor)
        {
            Console.ForegroundColor = foregroundColor;
        }

        public void Dispose()
        {
            Console.ResetColor();
        }
    }
}
