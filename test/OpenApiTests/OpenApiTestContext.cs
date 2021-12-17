using System.Text.Json;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;

namespace OpenApiTests;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public class OpenApiTestContext<TStartup, TDbContext> : IntegrationTestContext<TStartup, TDbContext>
    where TStartup : class
    where TDbContext : DbContext
{
    private const string GeneratedDocumentName = "swagger.g.json";

    internal readonly Lazy<Task<JsonElement>> LazyDocument;
    internal string? GeneratedDocumentNamespace;

    public OpenApiTestContext()
    {
        LazyDocument = new Lazy<Task<JsonElement>>(GetDocumentAsync, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    private async Task<JsonElement> GetDocumentAsync()
    {
        string content = await GetAsync("swagger/v1/swagger.json");

        JsonDocument document = JsonDocument.Parse(content);

        using (document)
        {
            JsonElement clonedDocument = document.RootElement.Clone();

            await WriteDocumentToFileAsync(clonedDocument);

            return clonedDocument;
        }
    }

    private async Task<string> GetAsync(string requestUrl)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

        using HttpClient client = Factory.CreateClient();
        using HttpResponseMessage responseMessage = await client.SendAsync(request);

        return await responseMessage.Content.ReadAsStringAsync();
    }

    public override void UseController<TController>()
    {
        if (!LazyDocument.IsValueCreated)
        {
            base.UseController<TController>();
        }
    }

    private async Task WriteDocumentToFileAsync(JsonElement document)
    {
        string pathToTestSuiteDirectory = GetTestSuitePath();
        string pathToDocument = Path.Join(pathToTestSuiteDirectory, GeneratedDocumentName);
        string json = document.ToString();
        await File.WriteAllTextAsync(pathToDocument, json);
    }

    private string GetTestSuitePath()
    {
        string pathToSolutionTestDirectory = Path.Combine(Environment.CurrentDirectory, "../../../../");
        pathToSolutionTestDirectory = Path.GetFullPath(pathToSolutionTestDirectory);

        if (GeneratedDocumentNamespace == null)
        {
            throw new Exception(
                $"Failed to write {GeneratedDocumentName} to disk. Ensure '{nameof(OpenApiTestContext<object, DbContext>)}.{nameof(GeneratedDocumentNamespace)}' is set.");
        }

        string pathToCurrentNamespaceRelativeToTestDirectory = Path.Combine(GeneratedDocumentNamespace.Split('.'));

        return Path.Join(pathToSolutionTestDirectory, pathToCurrentNamespaceRelativeToTestDirectory);
    }
}
