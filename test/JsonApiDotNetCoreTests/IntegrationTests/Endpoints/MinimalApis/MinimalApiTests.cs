using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.Endpoints.MinimalApis;

public sealed class MinimalApiTests : IClassFixture<IntegrationTestContext<TestableStartup<EmptyDbContext>, EmptyDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<EmptyDbContext>, EmptyDbContext> _testContext;
    private readonly MinimalApiFakers _fakers = new();

    public MinimalApiTests(IntegrationTestContext<TestableStartup<EmptyDbContext>, EmptyDbContext> testContext)
    {
        _testContext = testContext;

        testContext.ConfigureServices(services =>
        {
            services.AddSingleton<InMemoryOutgoingEmailsProvider>();
            services.TryAddEnumerable(ServiceDescriptor.Transient<IStartupFilter, MinimalApiStartupFilter>());
        });

        var emailsProvider = _testContext.App.Services.GetRequiredService<InMemoryOutgoingEmailsProvider>();
        emailsProvider.SentEmails.Clear();
    }

    [Fact]
    public async Task Can_send_email()
    {
        // Arrange
        var emailsProvider = _testContext.App.Services.GetRequiredService<InMemoryOutgoingEmailsProvider>();

        Email newEmail = _fakers.Email.GenerateOne();

        const string route = "/emails/send";

        using HttpClient httpClient = _testContext.App.GetTestClient();

        // Act
        using HttpResponseMessage response = await httpClient.PostAsJsonAsync(route, newEmail);

        // Assert
        response.ShouldHaveStatusCode(HttpStatusCode.OK);

        emailsProvider.SentEmails.Should().HaveCount(1);
    }

    [Fact]
    public async Task Cannot_send_email_with_invalid_addresses()
    {
        // Arrange
        Email newEmail = _fakers.Email.GenerateOne();
        newEmail.From = "invalid-sender-address";
        newEmail.To = "invalid-recipient-address";

        const string route = "/emails/send";

        using HttpClient httpClient = _testContext.App.GetTestClient();

        // Act
        using HttpResponseMessage response = await httpClient.PostAsJsonAsync(route, newEmail);

        // Assert
        response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails.Status.Should().Be((int)HttpStatusCode.BadRequest);
        problemDetails.Title.Should().Be("One or more validation errors occurred.");
        problemDetails.Detail.Should().BeNull();
        problemDetails.Instance.Should().BeNull();
        problemDetails.Errors.Should().HaveCount(2);

        string fromError = problemDetails.Errors.Should().ContainKey("From").WhoseValue.Should().ContainSingle().Which;
        fromError.Should().Be("The From field is not a valid e-mail address.");

        string toError = problemDetails.Errors.Should().ContainKey("To").WhoseValue.Should().ContainSingle().Which;
        toError.Should().Be("The To field is not a valid e-mail address.");
    }

    [Fact]
    public async Task Can_get_sent_emails()
    {
        // Arrange
        var timeProvider = _testContext.App.Services.GetRequiredService<TimeProvider>();
        var emailsProvider = _testContext.App.Services.GetRequiredService<InMemoryOutgoingEmailsProvider>();

        DateTimeOffset utcNow = timeProvider.GetUtcNow();

        Email existingEmail = _fakers.Email.GenerateOne();
        existingEmail.SetSentAt(utcNow.AddHours(-1));
        emailsProvider.SentEmails.TryAdd(utcNow, existingEmail);

        string sinceUtc = Uri.EscapeDataString(utcNow.AddHours(-2).ToString("O"));
        string route = $"/emails/sent-since?sinceUtc={sinceUtc}";

        using HttpClient httpClient = _testContext.App.GetTestClient();

        // Act
        using HttpResponseMessage response = await httpClient.GetAsync(route);

        // Assert
        response.ShouldHaveStatusCode(HttpStatusCode.OK);

        var emails = await response.Content.ReadFromJsonAsync<List<Email>>();
        emails.Should().HaveCount(1);
        emails[0].Subject.Should().Be(existingEmail.Subject);
        emails[0].Body.Should().Be(existingEmail.Body);
        emails[0].From.Should().Be(existingEmail.From);
        emails[0].To.Should().Be(existingEmail.To);
        emails[0].SentAtUtc.Should().Be(existingEmail.SentAtUtc);
    }

    [Fact]
    public async Task Cannot_get_sent_emails_in_future()
    {
        // Arrange
        var timeProvider = _testContext.App.Services.GetRequiredService<TimeProvider>();

        string sinceUtc = Uri.EscapeDataString(timeProvider.GetUtcNow().AddHours(1).ToString("O"));
        string route = $"/emails/sent-since?sinceUtc={sinceUtc}";

        using HttpClient httpClient = _testContext.App.GetTestClient();

        // Act
        using HttpResponseMessage response = await httpClient.GetAsync(route);

        // Assert
        response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails.Status.Should().Be((int)HttpStatusCode.BadRequest);
        problemDetails.Title.Should().Be("One or more validation errors occurred.");
        problemDetails.Detail.Should().BeNull();
        problemDetails.Instance.Should().BeNull();
        problemDetails.Errors.Should().HaveCount(1);

        string sinceUtcError = problemDetails.Errors.Should().ContainKey("sinceUtc").WhoseValue.Should().ContainSingle().Which;
        sinceUtcError.Should().Be("The sinceUtc parameter must be in the past.");
    }

    [Fact]
    public async Task Can_try_get_sent_emails_in_future()
    {
        // Arrange
        var timeProvider = _testContext.App.Services.GetRequiredService<TimeProvider>();

        string sinceUtc = Uri.EscapeDataString(timeProvider.GetUtcNow().AddHours(1).ToString("O"));
        string route = $"/emails/sent-since?sinceUtc={sinceUtc}";

        using var request = new HttpRequestMessage(HttpMethod.Head, route);

        using HttpClient httpClient = _testContext.App.GetTestClient();

        // Act
        using HttpResponseMessage response = await httpClient.SendAsync(request);

        // Assert
        response.ShouldHaveStatusCode(HttpStatusCode.BadRequest);
    }
}
