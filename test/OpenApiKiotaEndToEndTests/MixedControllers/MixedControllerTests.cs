using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Http.HttpClientLibrary;
using OpenApiKiotaEndToEndTests.MixedControllers.GeneratedCode;
using OpenApiKiotaEndToEndTests.MixedControllers.GeneratedCode.Models;
using OpenApiTests.MixedControllers;
using TestBuildingBlocks;
using Xunit;
using Xunit.Abstractions;
using ClientEmail = OpenApiKiotaEndToEndTests.MixedControllers.GeneratedCode.Models.Email;
using ServerEmail = OpenApiTests.MixedControllers.Email;

namespace OpenApiKiotaEndToEndTests.MixedControllers;

public sealed class MixedControllerTests : IClassFixture<IntegrationTestContext<MixedControllerStartup, CoffeeDbContext>>, IDisposable
{
    private readonly IntegrationTestContext<MixedControllerStartup, CoffeeDbContext> _testContext;
    private readonly TestableHttpClientRequestAdapterFactory _requestAdapterFactory;
    private readonly MixedControllerFakers _fakers = new();

    public MixedControllerTests(IntegrationTestContext<MixedControllerStartup, CoffeeDbContext> testContext, ITestOutputHelper testOutputHelper)
    {
        _testContext = testContext;
        _requestAdapterFactory = new TestableHttpClientRequestAdapterFactory(testOutputHelper);

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
        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new MixedControllersClient(requestAdapter);

        byte[] fileContents = "Hello upload"u8.ToArray();
        using var stream = new MemoryStream();
        stream.Write(fileContents);
        stream.Seek(0, SeekOrigin.Begin);

        var requestBody = new MultipartBody();
        requestBody.AddOrReplacePart("file", "text/plain", stream, "demo-upload.txt");

        // Act
        string? response = await apiClient.FileTransfers.PostAsync(requestBody);

        // Assert
        response.Should().Be($"Received file with a size of {fileContents.Length} bytes.");
    }

    [Fact]
    public async Task Cannot_upload_empty_file()
    {
        // Arrange
        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new MixedControllersClient(requestAdapter);

        using var stream = new MemoryStream();
        var requestBody = new MultipartBody();
        requestBody.AddOrReplacePart("file", "text/plain", stream, "demo-empty.txt");

        // Act
        Func<Task> action = async () => await apiClient.FileTransfers.PostAsync(requestBody);

        // Assert
        ApiException exception = (await action.Should().ThrowExactlyAsync<ApiException>()).Which;
        exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Finds_existing_file()
    {
        // Arrange
        byte[] fileContents = "Hello find"u8.ToArray();

        var storage = _testContext.Factory.Services.GetRequiredService<InMemoryFileStorage>();
        storage.Files.TryAdd("demo-existing-file.txt", fileContents);

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new MixedControllersClient(requestAdapter);

        // Act
        Func<Task> action = async () => await apiClient.FileTransfers.Find.GetAsync(request => request.QueryParameters.FileName = "demo-existing-file.txt");

        // Assert
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Does_not_find_missing_file()
    {
        // Arrange
        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new MixedControllersClient(requestAdapter);

        // Act
        Func<Task> action = async () => await apiClient.FileTransfers.Find.GetAsync(request => request.QueryParameters.FileName = "demo-missing-file.txt");

        // Assert
        ApiException exception = (await action.Should().ThrowExactlyAsync<ApiException>()).Which;
        exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Can_download_file()
    {
        // Arrange
        byte[] fileContents = "Hello download"u8.ToArray();

        var storage = _testContext.Factory.Services.GetRequiredService<InMemoryFileStorage>();
        storage.Files.TryAdd("demo-download.txt", fileContents);

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new MixedControllersClient(requestAdapter);

        // Act
        await using Stream? response = await apiClient.FileTransfers.GetAsync(request => request.QueryParameters.FileName = "demo-download.txt");

        // Assert
        response.Should().NotBeNull();

        using var streamReader = new StreamReader(response);
        string downloadedContents = await streamReader.ReadToEndAsync();

        downloadedContents.Should().Be("Hello download");
    }

    [Fact]
    public async Task Cannot_download_missing_file()
    {
        // Arrange
        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new MixedControllersClient(requestAdapter);

        // Act
        Func<Task> action = async () => await apiClient.FileTransfers.GetAsync(request => request.QueryParameters.FileName = "demo-missing-file.txt");

        // Assert
        ApiException exception = (await action.Should().ThrowExactlyAsync<ApiException>()).Which;
        exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Can_send_email()
    {
        // Arrange
        var emailsProvider = _testContext.Factory.Services.GetRequiredService<InMemoryOutgoingEmailsProvider>();

        ServerEmail newEmail = _fakers.Email.GenerateOne();

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new MixedControllersClient(requestAdapter);

        var requestBody = new ClientEmail
        {
            Subject = newEmail.Subject,
            Body = newEmail.Body,
            From = newEmail.From,
            To = newEmail.To
        };

        // Act
        await apiClient.Emails.Send.PostAsync(requestBody);

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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new MixedControllersClient(requestAdapter);

        var requestBody = new ClientEmail
        {
            Subject = newEmail.Subject,
            Body = newEmail.Body,
            From = newEmail.From,
            To = newEmail.To
        };

        // Act
        Func<Task> action = async () => await apiClient.Emails.Send.PostAsync(requestBody);

        // Assert
        HttpValidationProblemDetails exception = (await action.Should().ThrowExactlyAsync<HttpValidationProblemDetails>()).Which;

        exception.Status.Should().Be((int)HttpStatusCode.BadRequest);
        exception.Type.Should().Be("https://tools.ietf.org/html/rfc9110#section-15.5.1");
        exception.Title.Should().Be("One or more validation errors occurred.");
        exception.Detail.Should().BeNull();
        exception.Instance.Should().BeNull();
        exception.Errors.Should().NotBeNull();

        IDictionary<string, object> errors = exception.Errors.AdditionalData.Should().HaveCount(2).And.Subject;

        errors.Should().ContainKey("From").WhoseValue.Should().BeOfType<UntypedArray>().Subject.GetValue().ToArray().Should().ContainSingle().Which.Should()
            .BeOfType<UntypedString>().Subject.GetValue().Should().Be("The From field is not a valid e-mail address.");

        errors.Should().ContainKey("To").WhoseValue.Should().BeOfType<UntypedArray>().Subject.GetValue().ToArray().Should().ContainSingle().Which.Should()
            .BeOfType<UntypedString>().Subject.GetValue().Should().Be("The To field is not a valid e-mail address.");
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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new MixedControllersClient(requestAdapter);

        DateTimeOffset sinceUtc = utcNow.AddHours(-2);

        // Act
        List<ClientEmail>? response = await apiClient.Emails.SentSince.GetAsync(request => request.QueryParameters.SinceUtc = sinceUtc);

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

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new MixedControllersClient(requestAdapter);

        DateTimeOffset sinceUtc = timeProvider.GetUtcNow().AddHours(1);

        // Act
        Func<Task> action = async () => await apiClient.Emails.SentSince.GetAsync(request => request.QueryParameters.SinceUtc = sinceUtc);

        // Assert
        HttpValidationProblemDetails exception = (await action.Should().ThrowExactlyAsync<HttpValidationProblemDetails>()).Which;

        exception.Status.Should().Be((int)HttpStatusCode.BadRequest);
        exception.Type.Should().Be("https://tools.ietf.org/html/rfc9110#section-15.5.1");
        exception.Title.Should().Be("One or more validation errors occurred.");
        exception.Detail.Should().BeNull();
        exception.Instance.Should().BeNull();
        exception.Errors.Should().NotBeNull();

        IDictionary<string, object>? errors = exception.Errors.AdditionalData.Should().HaveCount(1).And.Subject;

        errors.Should().ContainKey("sinceUtc").WhoseValue.Should().BeOfType<UntypedArray>().Subject.GetValue().ToArray().Should().ContainSingle().Which.Should()
            .BeOfType<UntypedString>().Subject.GetValue().Should().Be("The sinceUtc parameter must be in the past.");
    }

    [Fact]
    public async Task Can_try_get_sent_emails_in_future()
    {
        // Arrange
        var timeProvider = _testContext.Factory.Services.GetRequiredService<TimeProvider>();

        using HttpClientRequestAdapter requestAdapter = _requestAdapterFactory.CreateAdapter(_testContext.Factory);
        var apiClient = new MixedControllersClient(requestAdapter);

        DateTimeOffset sinceUtc = timeProvider.GetUtcNow().AddHours(1);

        // Act
        Func<Task> action = async () => await apiClient.Emails.SentSince.HeadAsync(request => request.QueryParameters.SinceUtc = sinceUtc);

        // Assert
        ApiException exception = (await action.Should().ThrowExactlyAsync<ApiException>()).Which;

        exception.ResponseStatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        exception.ResponseHeaders.Should().BeEmpty();
    }

    public void Dispose()
    {
        _requestAdapterFactory.Dispose();
    }
}
