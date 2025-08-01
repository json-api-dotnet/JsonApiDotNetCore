using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.OpenApi.Client.NSwag;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenApiNSwagEndToEndTests.MixedControllers.GeneratedCode;
using OpenApiTests.MixedControllers;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;
using ClientEmail = OpenApiNSwagEndToEndTests.MixedControllers.GeneratedCode.Email;
using ServerEmail = OpenApiTests.MixedControllers.Email;

namespace OpenApiNSwagEndToEndTests.MixedControllers;

public sealed class MixedControllerTests : IClassFixture<IntegrationTestContext<MixedControllerStartup, CoffeeDbContext>>, IDisposable
{
    private readonly IntegrationTestContext<MixedControllerStartup, CoffeeDbContext> _testContext;
    private readonly XUnitLogHttpMessageHandler _logHttpMessageHandler;
    private readonly MixedControllerFakers _fakers = new();

    public MixedControllerTests(IntegrationTestContext<MixedControllerStartup, CoffeeDbContext> testContext, ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _logHttpMessageHandler = new XUnitLogHttpMessageHandler(testOutputHelper);

        testContext.UseController<FileTransferController>();
        testContext.UseController<CupOfCoffeesController>();

        testContext.ConfigureServices(services =>
        {
            services.AddSingleton<InMemoryFileStorage>();
            services.AddSingleton<InMemoryOutgoingEmailsProvider>();
            services.TryAddEnumerable(ServiceDescriptor.Transient<IStartupFilter, MinimalApiStartupFilter>());
        });

        var fileStorage = _testContext.Factory.Services.GetRequiredService<InMemoryFileStorage>();
        fileStorage.Files.Clear();

        var emailsProvider = _testContext.Factory.Services.GetRequiredService<InMemoryOutgoingEmailsProvider>();
        emailsProvider.SentEmails.Clear();
    }

    [Fact]
    public async Task Can_upload_file()
    {
        // Arrange
        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new MixedControllersClient(httpClient);

        byte[] fileContents = "Hello upload"u8.ToArray();
        using var stream = new MemoryStream();
        stream.Write(fileContents);
        stream.Seek(0, SeekOrigin.Begin);
        var fileParameter = new FileParameter(stream, "demo-upload.txt", "text/plain");

        // Act
        string response = await apiClient.UploadAsync(fileParameter);

        // Assert
        response.Should().Be($"Received file with a size of {fileContents.Length} bytes.");
    }

    [Fact]
    public async Task Cannot_upload_empty_file()
    {
        // Arrange
        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new MixedControllersClient(httpClient);

        using var stream = new MemoryStream();
        var fileParameter = new FileParameter(stream, "demo-empty.txt", "text/plain");

        // Act
        Func<Task> action = async () => await apiClient.UploadAsync(fileParameter);

        // Assert
        ApiException exception = (await action.Should().ThrowExactlyAsync<ApiException>()).Which;

        exception.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        exception.Message.Should().Be("HTTP 400: Bad Request");
        exception.Response.Should().Be("Empty files cannot be uploaded.");
    }

    [Fact]
    public async Task Finds_existing_file()
    {
        // Arrange
        byte[] fileContents = "Hello find"u8.ToArray();

        var storage = _testContext.Factory.Services.GetRequiredService<InMemoryFileStorage>();
        storage.Files.TryAdd("demo-existing-file.txt", fileContents);

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new MixedControllersClient(httpClient);

        // Act
        Func<Task> action = async () => await apiClient.ExistsAsync("demo-existing-file.txt");

        // Assert
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Does_not_find_missing_file()
    {
        // Arrange
        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new MixedControllersClient(httpClient);

        // Act
        Func<Task> action = async () => await apiClient.ExistsAsync("demo-missing-file.txt");

        // Assert
        ApiException exception = (await action.Should().ThrowExactlyAsync<ApiException>()).Which;

        exception.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        exception.Message.Should().Be("HTTP 404: Not Found");
        exception.Response.Should().BeNull();
    }

    [Fact]
    public async Task Can_download_file()
    {
        // Arrange
        byte[] fileContents = "Hello download"u8.ToArray();

        var storage = _testContext.Factory.Services.GetRequiredService<InMemoryFileStorage>();
        storage.Files.TryAdd("demo-download.txt", fileContents);

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new MixedControllersClient(httpClient);

        // Act
        using FileResponse response = await apiClient.DownloadAsync("demo-download.txt");

        // Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        response.Headers.Should().ContainKey("Content-Type").WhoseValue.Should().ContainSingle().Which.Should().Be("application/octet-stream");
        response.Headers.Should().ContainKey("Content-Length").WhoseValue.Should().ContainSingle().Which.Should().Be(fileContents.Length.ToString());

        using var streamReader = new StreamReader(response.Stream);
        string downloadedContents = await streamReader.ReadToEndAsync();

        downloadedContents.Should().Be("Hello download");
    }

    [Fact]
    public async Task Cannot_download_missing_file()
    {
        // Arrange
        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new MixedControllersClient(httpClient);

        // Act
        Func<Task> action = async () => await apiClient.DownloadAsync("demo-missing-file.txt");

        // Assert
        ApiException exception = (await action.Should().ThrowExactlyAsync<ApiException>()).Which;

        exception.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        exception.Message.Should().Be("HTTP 404: Not Found");
        exception.Response.Should().Be("The file 'demo-missing-file.txt' does not exist.");
    }

    [Fact]
    public async Task Can_send_email()
    {
        // Arrange
        var emailsProvider = _testContext.Factory.Services.GetRequiredService<InMemoryOutgoingEmailsProvider>();

        ServerEmail newEmail = _fakers.Email.GenerateOne();

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new MixedControllersClient(httpClient);

        var requestBody = new ClientEmail
        {
            Subject = newEmail.Subject,
            Body = newEmail.Body,
            From = newEmail.From,
            To = newEmail.To
        };

        // Act
        await apiClient.SendEmailAsync(requestBody);

        // Assert
        emailsProvider.SentEmails.Should().HaveCount(1);
    }

    [Fact]
    public async Task Cannot_send_email_with_invalid_addresses()
    {
        // Arrange
        ServerEmail newEmail = _fakers.Email.GenerateOne();
        newEmail.From = "invalid-sender-address";
        newEmail.To = "invalid-recipient-address";

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new MixedControllersClient(httpClient);

        var requestBody = new ClientEmail
        {
            Subject = newEmail.Subject,
            Body = newEmail.Body,
            From = newEmail.From,
            To = newEmail.To
        };

        // Act
        Func<Task> action = async () => await apiClient.SendEmailAsync(requestBody);

        // Assert
        ApiException<HttpValidationProblemDetails> exception = (await action.Should().ThrowExactlyAsync<ApiException<HttpValidationProblemDetails>>()).Which;

        exception.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        exception.Message.Should().Be("HTTP 400: Bad Request");
        exception.Result.Status.Should().Be((int)HttpStatusCode.BadRequest);
        exception.Result.Type.Should().Be("https://tools.ietf.org/html/rfc9110#section-15.5.1");
        exception.Result.Title.Should().Be("One or more validation errors occurred.");
        exception.Result.Detail.Should().BeNull();
        exception.Result.Instance.Should().BeNull();

        IDictionary<string, ICollection<string>> errors = exception.Result.Errors.Should().HaveCount(2).And.Subject;
        errors.Should().ContainKey("From").WhoseValue.Should().ContainSingle().Which.Should().Be("The From field is not a valid e-mail address.");
        errors.Should().ContainKey("To").WhoseValue.Should().ContainSingle().Which.Should().Be("The To field is not a valid e-mail address.");
    }

    [Fact]
    public async Task Can_get_sent_emails()
    {
        // Arrange
        var timeProvider = _testContext.Factory.Services.GetRequiredService<TimeProvider>();
        var emailsProvider = _testContext.Factory.Services.GetRequiredService<InMemoryOutgoingEmailsProvider>();

        DateTimeOffset utcNow = timeProvider.GetUtcNow();

        ServerEmail existingEmail = _fakers.Email.GenerateOne();
        existingEmail.SetSentAt(utcNow.AddHours(-1));
        emailsProvider.SentEmails.TryAdd(utcNow, existingEmail);

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new MixedControllersClient(httpClient);

        DateTimeOffset sinceUtc = utcNow.AddHours(-2);

        // Act
        ICollection<ClientEmail> response = await apiClient.GetSentSinceAsync(sinceUtc);

        // Assert
        response.Should().HaveCount(1);
        response.ElementAt(0).Subject.Should().Be(existingEmail.Subject);
        response.ElementAt(0).Body.Should().Be(existingEmail.Body);
        response.ElementAt(0).From.Should().Be(existingEmail.From);
        response.ElementAt(0).To.Should().Be(existingEmail.To);
        response.ElementAt(0).SentAtUtc.Should().Be(existingEmail.SentAtUtc);
    }

    [Fact]
    public async Task Cannot_get_sent_emails_in_future()
    {
        // Arrange
        var timeProvider = _testContext.Factory.Services.GetRequiredService<TimeProvider>();

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new MixedControllersClient(httpClient);

        DateTimeOffset sinceUtc = timeProvider.GetUtcNow().AddHours(1);

        // Act
        Func<Task> action = async () => await apiClient.GetSentSinceAsync(sinceUtc);

        // Assert
        ApiException<HttpValidationProblemDetails> exception = (await action.Should().ThrowExactlyAsync<ApiException<HttpValidationProblemDetails>>()).Which;

        exception.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        exception.Message.Should().Be("HTTP 400: Bad Request");
        exception.Result.Status.Should().Be((int)HttpStatusCode.BadRequest);
        exception.Result.Type.Should().Be("https://tools.ietf.org/html/rfc9110#section-15.5.1");
        exception.Result.Title.Should().Be("One or more validation errors occurred.");
        exception.Result.Detail.Should().BeNull();
        exception.Result.Instance.Should().BeNull();

        IDictionary<string, ICollection<string>> errors = exception.Result.Errors.Should().HaveCount(1).And.Subject;
        errors.Should().ContainKey("sinceUtc").WhoseValue.Should().ContainSingle().Which.Should().Be("The sinceUtc parameter must be in the past.");
    }

    [Fact]
    public async Task Can_try_get_sent_emails_in_future()
    {
        // Arrange
        var timeProvider = _testContext.Factory.Services.GetRequiredService<TimeProvider>();

        using HttpClient httpClient = _testContext.Factory.CreateDefaultClient(_logHttpMessageHandler);
        var apiClient = new MixedControllersClient(httpClient);

        DateTimeOffset sinceUtc = timeProvider.GetUtcNow().AddHours(1);

        // Act
        Func<Task> action = async () => await apiClient.TryGetSentSinceAsync(sinceUtc);

        // Assert
        ApiException exception = (await action.Should().ThrowExactlyAsync<ApiException>()).Which;

        exception.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        exception.Message.Should().Be("HTTP 400: Bad Request");
        exception.Response.Should().BeNull();
    }

    public void Dispose()
    {
        _logHttpMessageHandler.Dispose();
    }
}
