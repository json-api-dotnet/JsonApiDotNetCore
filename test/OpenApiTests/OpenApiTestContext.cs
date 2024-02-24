using System.Reflection;
using System.Text.Json;
using JetBrains.Annotations;
using TestBuildingBlocks;

namespace OpenApiTests;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public class OpenApiTestContext<TStartup, TDbContext> : IntegrationTestContext<TStartup, TDbContext>
    where TStartup : class
    where TDbContext : TestableDbContext
{
    private readonly Lazy<Task<JsonElement>> _lazySwaggerDocument;

    internal string? SwaggerDocumentOutputDirectory { get; set; }

    public OpenApiTestContext()
    {
        _lazySwaggerDocument = new Lazy<Task<JsonElement>>(CreateSwaggerDocumentAsync, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    internal Task<JsonElement> GetSwaggerDocumentAsync()
    {
        return _lazySwaggerDocument.Value;
    }

    private async Task<JsonElement> CreateSwaggerDocumentAsync()
    {
        string content = await GetAsync("swagger/v1/swagger.json");

        JsonElement rootElement = ParseSwaggerDocument(content);

        if (SwaggerDocumentOutputDirectory != null)
        {
            string absoluteOutputPath = GetSwaggerDocumentAbsoluteOutputPath(SwaggerDocumentOutputDirectory);
            await WriteToDiskAsync(absoluteOutputPath, rootElement);
        }

        return rootElement;
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

    private static Task WriteToDiskAsync(string path, JsonElement jsonElement)
    {
        string directory = Path.GetDirectoryName(path)!;
        Directory.CreateDirectory(directory);

        string contents = jsonElement.ToString();
        return File.WriteAllTextAsync(path, contents);
    }
}
