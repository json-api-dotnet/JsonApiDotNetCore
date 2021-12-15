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
    internal readonly Lazy<Task<JsonElement>> LazyDocument;

    public OpenApiTestContext()
    {
        LazyDocument = new Lazy<Task<JsonElement>>(GetDocumentAsync, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    private async Task<JsonElement> GetDocumentAsync()
    {
        string content = await GetAsync("swagger/v1/swagger.json");

        JsonDocument parsedContent = JsonDocument.Parse(content);

        using (parsedContent)
        {
            JsonElement clonedRoot = parsedContent.RootElement.Clone();
            return clonedRoot;
        }
    }

    private async Task<string> GetAsync(string requestUrl)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

        using HttpClient client = Factory.CreateClient();
        using HttpResponseMessage responseMessage = await client.SendAsync(request);

        return await responseMessage.Content.ReadAsStringAsync();
    }
}
