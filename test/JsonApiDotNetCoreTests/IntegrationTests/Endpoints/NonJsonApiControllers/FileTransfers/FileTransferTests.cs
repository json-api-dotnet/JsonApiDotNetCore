using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.Endpoints.NonJsonApiControllers.FileTransfers;

public sealed class FileTransferTests : IClassFixture<IntegrationTestContext<TestableStartup<EmptyDbContext>, EmptyDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<EmptyDbContext>, EmptyDbContext> _testContext;

    public FileTransferTests(IntegrationTestContext<TestableStartup<EmptyDbContext>, EmptyDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<FileTransferController>();

        testContext.ConfigureServices(services => services.AddSingleton<InMemoryFileStorage>());

        var fileStorage = _testContext.App.Services.GetRequiredService<InMemoryFileStorage>();
        fileStorage.Files.Clear();
    }

    [Fact]
    public async Task Can_upload_file()
    {
        // Arrange
        byte[] data = "Hello upload"u8.ToArray();
        using var fileContent = new ByteArrayContent(data);

        using var content = new MultipartFormDataContent();
        content.Add(fileContent, "file", "demo-upload.txt");

        const string route = "/fileTransfers";

        using HttpClient httpClient = _testContext.App.GetTestClient();

        // Act
        using HttpResponseMessage response = await httpClient.PostAsync(route, content);

        // Assert
        response.ShouldHaveStatusCode(HttpStatusCode.OK);

        string responseText = await response.Content.ReadAsStringAsync();
        responseText.Should().Be($"Received file with a size of {data.Length} bytes.");
    }

    [Fact]
    public async Task Cannot_upload_empty_file()
    {
        // Arrange
        using var fileContent = new ByteArrayContent([]);

        using var content = new MultipartFormDataContent();
        content.Add(fileContent, "file", "demo-empty.txt");

        const string route = "/fileTransfers";

        using HttpClient httpClient = _testContext.App.GetTestClient();

        // Act
        using HttpResponseMessage response = await httpClient.PostAsync(route, content);

        // Assert
        response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        string responseText = await response.Content.ReadAsStringAsync();
        responseText.Should().Be("Empty files cannot be uploaded.");
    }

    [Fact]
    public async Task Finds_existing_file()
    {
        // Arrange
        byte[] data = "Hello find"u8.ToArray();

        var storage = _testContext.App.Services.GetRequiredService<InMemoryFileStorage>();
        storage.Files.TryAdd("demo-existing-file.txt", data);

        const string route = "/fileTransfers/find?fileName=demo-existing-file.txt";

        using HttpClient httpClient = _testContext.App.GetTestClient();

        // Act
        using HttpResponseMessage response = await httpClient.GetAsync(route);

        // Assert
        response.ShouldHaveStatusCode(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Does_not_find_missing_file()
    {
        // Arrange
        const string route = "/fileTransfers/find?fileName=demo-missing-file.txt";

        using HttpClient httpClient = _testContext.App.GetTestClient();

        // Act
        using HttpResponseMessage response = await httpClient.GetAsync(route);

        // Assert
        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Can_download_file()
    {
        // Arrange
        byte[] data = "Hello download"u8.ToArray();

        var storage = _testContext.App.Services.GetRequiredService<InMemoryFileStorage>();
        storage.Files.TryAdd("demo-download.txt", data);

        const string route = "/fileTransfers?fileName=demo-download.txt";

        using HttpClient httpClient = _testContext.App.GetTestClient();

        // Act
        using HttpResponseMessage response = await httpClient.GetAsync(route);

        // Assert
        response.ShouldHaveStatusCode(HttpStatusCode.OK);

        response.Content.Headers.ContentType.Should().NotBeNull();
        response.Content.Headers.ContentType.ToString().Should().Be("application/octet-stream");
        response.Content.Headers.ContentLength.Should().Be(data.Length);

        string responseText = await response.Content.ReadAsStringAsync();
        responseText.Should().Be("Hello download");
    }

    [Fact]
    public async Task Cannot_download_missing_file()
    {
        // Arrange
        const string route = "/fileTransfers?fileName=demo-missing-file.txt";

        using HttpClient httpClient = _testContext.App.GetTestClient();

        // Act
        using HttpResponseMessage response = await httpClient.GetAsync(route);

        // Assert
        response.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        string responseText = await response.Content.ReadAsStringAsync();
        responseText.Should().Be("The file 'demo-missing-file.txt' does not exist.");
    }
}
