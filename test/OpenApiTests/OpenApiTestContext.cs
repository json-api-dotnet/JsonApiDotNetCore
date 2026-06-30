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
    private readonly Lazy<Task<JsonElement>> _lazyDocument;
    private ITestOutputHelper? _testOutputHelper;

    internal string? OpenApiDocumentOutputDirectory { get; set; }

    public OpenApiTestContext()
    {
        _lazyDocument = new Lazy<Task<JsonElement>>(CreateOpenApiDocumentAsync, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    internal async Task<JsonElement> GetOpenApiDocumentAsync()
    {
        return await _lazyDocument.Value;
    }

    internal async Task<JsonElement> CreateOpenApiDocumentAsync()
    {
        string content = await GetAsync("/swagger/v1/swagger.json");

        JsonElement rootElement = ParseDocument(content);

        string? absoluteOutputPath = GetAbsoluteOutputPath();
        await WriteToDiskAsync(absoluteOutputPath, rootElement);

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

    private string? GetAbsoluteOutputPath()
    {
        if (OpenApiDocumentOutputDirectory != null)
        {
#if NET11_0
            Version frameworkVersion = typeof(object).Assembly.GetName().Version!;
            string targetFrameworkName = $"net{frameworkVersion.Major}.{frameworkVersion.Minor}";

            string testRootDirectory = Path.Combine(typeof(TDbContext).Assembly.Location, "../../../../../");
            string outputPath = Path.Combine(testRootDirectory, OpenApiDocumentOutputDirectory, targetFrameworkName, "swagger.g.json");

            return Path.GetFullPath(outputPath);
#elif NET11_0_OR_GREATER
#error Unsupported newer target framework. Please update the preprocessor directives in this file and references in consuming projects.
#else
            // Not writing to disk for simplicity and performance on lower target frameworks.
#endif
        }

        return null;
    }

    private async Task<string> GetAsync(string requestUrl)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

        using HttpClient client = Factory.CreateClient();
        using HttpResponseMessage responseMessage = await client.SendAsync(request);

        return await responseMessage.Content.ReadAsStringAsync();
    }

    private static JsonElement ParseDocument(string content)
    {
        using JsonDocument jsonDocument = JsonDocument.Parse(content);
        return jsonDocument.RootElement.Clone();
    }

    private static async Task WriteToDiskAsync(string? path, JsonElement jsonElement)
    {
        if (path != null)
        {
            string directory = Path.GetDirectoryName(path)!;
            Directory.CreateDirectory(directory);

            string newContent = jsonElement.ToString();

#if !DEBUG
            // Fail test when changes to the generated OpenAPI document haven't been committed in PRs.
            bool fileExists = File.Exists(path);
            string oldContent = fileExists ? await File.ReadAllTextAsync(path) : string.Empty;
#endif

            await File.WriteAllTextAsync(path, newContent);

#if !DEBUG
            if (!fileExists)
            {
                throw new InvalidOperationException($"""WARNING: File "{path}" is missing. Please commit the new file.""");
            }

            if (newContent != oldContent)
            {
                throw new InvalidOperationException($"""WARNING: File "{path}" has changed. Please commit the changes.""");
            }
#endif
        }
    }
}
