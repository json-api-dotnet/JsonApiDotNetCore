using System.Text.Json;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;
using Xunit;

namespace OpenApiTests;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class OpenApiTestContext<TStartup, TDbContext> : IntegrationTestContext<TStartup, TDbContext>, IAsyncLifetime
    where TStartup : class
    where TDbContext : DbContext
{
    internal JsonElement Document { get; private set; }

    public async Task InitializeAsync()
    {
        UseController<SupermarketsController>();

        string content = await GetAsync("swagger/v1/swagger.json");

        await WriteSwaggerDocumentToFileAsync(content);

        JsonDocument parsedContent = JsonDocument.Parse(content);

        Document = parsedContent.ToJsonElement();
    }

    private async Task<string> GetAsync(string requestUrl)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

        using HttpClient client = Factory.CreateClient();
        using HttpResponseMessage responseMessage = await client.SendAsync(request);

        return await responseMessage.Content.ReadAsStringAsync();
    }

    private async Task WriteSwaggerDocumentToFileAsync(string document)
    {
        string testSuitePath = GetTestSuitePath();
        string documentPath = Path.Join(testSuitePath, "swagger.json");
        await File.WriteAllTextAsync(documentPath, document);
    }

    private string GetTestSuitePath()
    {
        string solutionTestDirectoryPath = Directory.GetParent(Environment.CurrentDirectory)!.Parent!.Parent!.Parent!.FullName;
        string currentNamespacePathRelativeToTestDirectory = Path.Join(typeof(TStartup).Namespace!.Split('.'));

        return Path.Join(solutionTestDirectoryPath, currentNamespacePathRelativeToTestDirectory);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}
