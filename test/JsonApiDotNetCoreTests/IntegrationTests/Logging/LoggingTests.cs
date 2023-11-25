using System.Globalization;
using System.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.Logging;

public sealed class LoggingTests : IClassFixture<IntegrationTestContext<TestableStartup<LoggingDbContext>, LoggingDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<LoggingDbContext>, LoggingDbContext> _testContext;
    private readonly LoggingFakers _fakers = new();

    public LoggingTests(IntegrationTestContext<TestableStartup<LoggingDbContext>, LoggingDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<AuditEntriesController>();
        testContext.UseController<FruitBowlsController>();

        var loggerFactory = new FakeLoggerFactory(LogLevel.Trace);

        testContext.ConfigureLogging(options =>
        {
            options.ClearProviders();
            options.AddProvider(loggerFactory);
            options.SetMinimumLevel(LogLevel.Trace);
            options.AddFilter((category, _) => category != null && category.StartsWith("JsonApiDotNetCore.", StringComparison.Ordinal));
        });

        testContext.ConfigureServices(services =>
        {
            services.AddSingleton(loggerFactory);
        });
    }

    [Fact]
    public async Task Logs_request_body_at_Trace_level()
    {
        // Arrange
        var loggerFactory = _testContext.Factory.Services.GetRequiredService<FakeLoggerFactory>();
        loggerFactory.Logger.Clear();

        AuditEntry newEntry = _fakers.AuditEntry.Generate();

        var requestBody = new
        {
            data = new
            {
                type = "auditEntries",
                attributes = new
                {
                    userName = newEntry.UserName,
                    createdAt = newEntry.CreatedAt
                }
            }
        };

        // Arrange
        const string route = "/auditEntries";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        IReadOnlyList<string> logLines = loggerFactory.Logger.GetLines();
        logLines.ShouldNotBeEmpty();

        logLines.Should().ContainSingle(line =>
            line.StartsWith("[TRACE] Received POST request at 'http://localhost/auditEntries' with body: <<", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Logs_response_body_at_Trace_level()
    {
        // Arrange
        var loggerFactory = _testContext.Factory.Services.GetRequiredService<FakeLoggerFactory>();
        loggerFactory.Logger.Clear();

        // Arrange
        const string route = "/auditEntries";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteGetAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        IReadOnlyList<string> logLines = loggerFactory.Logger.GetLines();
        logLines.ShouldNotBeEmpty();

        logLines.Should().ContainSingle(line =>
            line.StartsWith("[TRACE] Sending 200 response for GET request at 'http://localhost/auditEntries' with body: <<", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Logs_invalid_request_body_error_at_Information_level()
    {
        // Arrange
        var loggerFactory = _testContext.Factory.Services.GetRequiredService<FakeLoggerFactory>();
        loggerFactory.Logger.Clear();

        // Arrange
        const string requestBody = "{ \"data\" {";

        const string route = "/auditEntries";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        IReadOnlyList<FakeLogMessage> logMessages = loggerFactory.Logger.GetMessages();
        logMessages.ShouldNotBeEmpty();

        logMessages.Should().ContainSingle(message => message.LogLevel == LogLevel.Information && message.Text.Contains("Failed to deserialize request body."));
    }

    [Fact]
    public async Task Logs_method_parameters_of_abstract_resource_type_at_Trace_level()
    {
        // Arrange
        var loggerFactory = _testContext.Factory.Services.GetRequiredService<FakeLoggerFactory>();
        loggerFactory.Logger.Clear();

        var existingBowl = new FruitBowl();
        Banana existingBanana = _fakers.Banana.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.FruitBowls.Add(existingBowl);
            dbContext.Fruits.Add(existingBanana);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "fruits",
                    id = existingBanana.StringId
                }
            }
        };

        string route = $"/fruitBowls/{existingBowl.StringId}/relationships/fruits";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        IReadOnlyList<string> logLines = loggerFactory.Logger.GetLines();

        logLines.Should().BeEquivalentTo(new[]
        {
            $@"[TRACE] Received POST request at 'http://localhost/fruitBowls/{existingBowl.StringId}/relationships/fruits' with body: <<{{
  ""data"": [
    {{
      ""type"": ""fruits"",
      ""id"": ""{existingBanana.StringId}""
    }}
  ]
}}>>",
            $@"[TRACE] Entering PostRelationshipAsync(id: {existingBowl.StringId}, relationshipName: ""fruits"", rightResourceIds: [
  {{
    ""ClrType"": ""{typeof(Fruit).FullName}"",
    ""StringId"": ""{existingBanana.StringId}""
  }}
])",
            $@"[TRACE] Entering AddToToManyRelationshipAsync(leftId: {existingBowl.StringId}, relationshipName: ""fruits"", rightResourceIds: [
  {{
    ""ClrType"": ""{typeof(Fruit).FullName}"",
    ""StringId"": ""{existingBanana.StringId}""
  }}
])",
            $@"[TRACE] Entering GetAsync(queryLayer: QueryLayer<Fruit>
{{
  Filter: equals(id,'{existingBanana.Id}')
  Selection
  {{
    FieldSelectors<Fruit>
    {{
      id
    }}
  }}
}}
)",
            $@"[TRACE] Entering ApplyQueryLayer(queryLayer: QueryLayer<Fruit>
{{
  Filter: equals(id,'{existingBanana.Id}')
  Selection
  {{
    FieldSelectors<Fruit>
    {{
      id
    }}
  }}
}}
)",
            $@"[TRACE] Entering AddToToManyRelationshipAsync(leftResource: null, leftId: {existingBowl.Id}, rightResourceIds: [
  {{
    ""Color"": ""Yellow"",
    ""LengthInCentimeters"": {existingBanana.LengthInCentimeters.ToString(CultureInfo.InvariantCulture)},
    ""Id"": {existingBanana.Id},
    ""StringId"": ""{existingBanana.StringId}""
  }}
])"
        }, options => options.Using(IgnoreLineEndingsComparer.Instance).WithStrictOrdering());
    }

    [Fact]
    public async Task Logs_method_parameters_of_concrete_resource_type_at_Trace_level()
    {
        // Arrange
        var loggerFactory = _testContext.Factory.Services.GetRequiredService<FakeLoggerFactory>();
        loggerFactory.Logger.Clear();

        var existingBowl = new FruitBowl();
        Peach existingPeach = _fakers.Peach.Generate();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.FruitBowls.Add(existingBowl);
            dbContext.Fruits.Add(existingPeach);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new[]
            {
                new
                {
                    type = "peaches",
                    id = existingPeach.StringId
                }
            }
        };

        string route = $"/fruitBowls/{existingBowl.StringId}/relationships/fruits";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        IReadOnlyList<string> logLines = loggerFactory.Logger.GetLines();

        logLines.Should().BeEquivalentTo(new[]
        {
            $@"[TRACE] Received POST request at 'http://localhost/fruitBowls/{existingBowl.StringId}/relationships/fruits' with body: <<{{
  ""data"": [
    {{
      ""type"": ""peaches"",
      ""id"": ""{existingPeach.StringId}""
    }}
  ]
}}>>",
            $@"[TRACE] Entering PostRelationshipAsync(id: {existingBowl.StringId}, relationshipName: ""fruits"", rightResourceIds: [
  {{
    ""Color"": ""Red/Yellow"",
    ""DiameterInCentimeters"": 0,
    ""Id"": {existingPeach.Id},
    ""StringId"": ""{existingPeach.StringId}""
  }}
])",
            $@"[TRACE] Entering AddToToManyRelationshipAsync(leftId: {existingBowl.StringId}, relationshipName: ""fruits"", rightResourceIds: [
  {{
    ""Color"": ""Red/Yellow"",
    ""DiameterInCentimeters"": 0,
    ""Id"": {existingPeach.Id},
    ""StringId"": ""{existingPeach.StringId}""
  }}
])",
            $@"[TRACE] Entering GetAsync(queryLayer: QueryLayer<Fruit>
{{
  Filter: equals(id,'{existingPeach.Id}')
  Selection
  {{
    FieldSelectors<Fruit>
    {{
      id
    }}
  }}
}}
)",
            $@"[TRACE] Entering ApplyQueryLayer(queryLayer: QueryLayer<Fruit>
{{
  Filter: equals(id,'{existingPeach.Id}')
  Selection
  {{
    FieldSelectors<Fruit>
    {{
      id
    }}
  }}
}}
)",
            $@"[TRACE] Entering AddToToManyRelationshipAsync(leftResource: null, leftId: {existingBowl.Id}, rightResourceIds: [
  {{
    ""Color"": ""Red/Yellow"",
    ""DiameterInCentimeters"": 0,
    ""Id"": {existingPeach.Id},
    ""StringId"": ""{existingPeach.StringId}""
  }}
])"
        }, options => options.Using(IgnoreLineEndingsComparer.Instance).WithStrictOrdering());
    }
}
