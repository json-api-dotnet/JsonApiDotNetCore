using System.Reflection;
using System.Text.Json;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TestBuildingBlocks;
using Xunit.Abstractions;

namespace OpenApiTests;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public class OpenApiTestContext<TStartup, TDbContext> : IntegrationTestContext<TStartup, TDbContext>
    where TStartup : class
    where TDbContext : TestableDbContext
{
    private readonly Lazy<Task<JsonElement>> _lazySwaggerDocument;
    private ITestOutputHelper? _testOutputHelper;

    internal string? SwaggerDocumentOutputDirectory { get; set; }

    public OpenApiTestContext()
    {
        _lazySwaggerDocument = new Lazy<Task<JsonElement>>(CreateSwaggerDocumentAsync, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    internal async Task<JsonElement> GetSwaggerDocumentAsync()
    {
        return await _lazySwaggerDocument.Value;
    }

    private async Task<JsonElement> CreateSwaggerDocumentAsync()
    {
        string content = await GetAsync("/swagger/v1/swagger.json");

        JsonElement rootElement = ParseSwaggerDocument(content);

        if (SwaggerDocumentOutputDirectory != null)
        {
            string absoluteOutputPath = GetSwaggerDocumentAbsoluteOutputPath(SwaggerDocumentOutputDirectory);
            await WriteToDiskAsync(absoluteOutputPath, rootElement);
        }

        return rootElement;
    }

    internal void SetTestOutputHelper(ITestOutputHelper testOutputHelper)
    {
        ArgumentNullException.ThrowIfNull(testOutputHelper);

        _testOutputHelper = testOutputHelper;
        ConfigureLogging(AddXUnitProvider);
    }

    private void AddXUnitProvider(ILoggingBuilder loggingBuilder)
    {
        if (_testOutputHelper != null)
        {
            loggingBuilder.SetMinimumLevel(LogLevel.Trace);
            loggingBuilder.Services.AddSingleton<ILoggerProvider>(_ => new XUnitLoggerProvider(_testOutputHelper, "JsonApiDotNetCore.OpenApi.Swashbuckle"));
        }
    }

    private static string GetSwaggerDocumentAbsoluteOutputPath(string relativePath)
    {
        string testRootDirectory = Path.Combine(Assembly.GetExecutingAssembly().Location, "../../../../../");
        string outputPath = Path.Combine(testRootDirectory, relativePath, "swagger.g.json");

        return Path.GetFullPath(outputPath);
    }

    private async Task<string> GetAsync(string requestUrl)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

        using HttpClient client = Factory.CreateClient();
        using HttpResponseMessage responseMessage = await client.SendAsync(request);

        return await responseMessage.Content.ReadAsStringAsync();
    }

    private static JsonElement ParseSwaggerDocument(string content)
    {
        using JsonDocument jsonDocument = JsonDocument.Parse(content);
        return jsonDocument.RootElement.Clone();
    }

    private static async Task WriteToDiskAsync(string path, JsonElement jsonElement)
    {
        string directory = Path.GetDirectoryName(path)!;
        Directory.CreateDirectory(directory);

        string contents = jsonElement.ToString();
        await File.WriteAllTextAsync(path, contents);
    }
}
