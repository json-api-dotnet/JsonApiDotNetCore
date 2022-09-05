using System.Reflection;
using System.Text.Json;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;
using SysNotNull = System.Diagnostics.CodeAnalysis.NotNullAttribute;

namespace OpenApiTests;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public class OpenApiTestContext<TStartup, TDbContext> : IntegrationTestContext<TStartup, TDbContext>
    where TStartup : class
    where TDbContext : DbContext
{
    private readonly Lazy<Task<JsonElement>> _lazySwaggerDocument;

    internal string? SwaggerDocumentOutputPath { private get; set; }

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
        string content = await GetAsync("swagger/v1/swagger.json");

        JsonElement rootElement = ParseSwaggerDocument(content);

        if (SwaggerDocumentOutputPath != null)
        {
            string absoluteOutputPath = GetSwaggerDocumentAbsoluteOutputPath(SwaggerDocumentOutputPath);
            await WriteToDiskAsync(absoluteOutputPath, rootElement);
        }

        return rootElement;
    }

    private static string GetSwaggerDocumentAbsoluteOutputPath(string relativePath)
    {
        string solutionRoot = Path.Combine(Assembly.GetExecutingAssembly().Location, "../../../../../../");
        string outputPath = Path.Combine(solutionRoot, relativePath, "swagger.g.json");

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
        string contents = jsonElement.ToString();
        await File.WriteAllTextAsync(path, contents);
    }
}
