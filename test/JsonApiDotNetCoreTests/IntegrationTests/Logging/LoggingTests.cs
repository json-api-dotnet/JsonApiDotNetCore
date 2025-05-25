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

        testContext.ConfigureLogging(options =>
        {
            var loggerProvider = new CapturingLoggerProvider((category, level) =>
                level >= LogLevel.Trace && category.StartsWith("JsonApiDotNetCore.", StringComparison.Ordinal));

            options.AddProvider(loggerProvider);
            options.SetMinimumLevel(LogLevel.Trace);

            options.Services.AddSingleton(loggerProvider);
        });
    }

    [Fact]
    public async Task Logs_request_body_at_Trace_level()
    {
        // Arrange
        var loggerProvider = _testContext.Factory.Services.GetRequiredService<CapturingLoggerProvider>();
        loggerProvider.Clear();

        AuditEntry newEntry = _fakers.AuditEntry.GenerateOne();

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

        IReadOnlyList<string> logLines = loggerProvider.GetLines();

        logLines.Should().ContainSingle(line =>
            line.StartsWith("[TRACE] Received POST request at 'http://localhost/auditEntries' with body: <<", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Logs_response_body_at_Trace_level()
    {
        // Arrange
        var loggerProvider = _testContext.Factory.Services.GetRequiredService<CapturingLoggerProvider>();
        loggerProvider.Clear();

        // Arrange
        const string route = "/auditEntries";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteGetAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        IReadOnlyList<string> logLines = loggerProvider.GetLines();

        logLines.Should().ContainSingle(line =>
            line.StartsWith("[TRACE] Sending 200 response for GET request at 'http://localhost/auditEntries' with body: <<", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Logs_invalid_request_body_error_at_Information_level()
    {
        // Arrange
        var loggerProvider = _testContext.Factory.Services.GetRequiredService<CapturingLoggerProvider>();
        loggerProvider.Clear();

        // Arrange
        const string requestBody = "{ \"data\" {";

        const string route = "/auditEntries";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        LogMessage[] infoMessages = loggerProvider.GetMessages().Where(message => message.LogLevel == LogLevel.Information).ToArray();
        infoMessages.Should().ContainSingle(message => message.Text.Contains("Failed to deserialize request body."));
    }

    [Fact]
    public async Task Logs_method_parameters_of_abstract_resource_type_at_Trace_level()
    {
        // Arrange
        var loggerProvider = _testContext.Factory.Services.GetRequiredService<CapturingLoggerProvider>();
        loggerProvider.Clear();

        var existingBowl = new FruitBowl();
        Banana existingBanana = _fakers.Banana.GenerateOne();

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

        string[] traceLines = loggerProvider.GetMessages().Where(message => message.LogLevel == LogLevel.Trace).Select(message => message.ToString()).ToArray();

        traceLines.Should().BeEquivalentTo(new[]
        {
            $$"""
            [TRACE] Received POST request at 'http://localhost/fruitBowls/{{existingBowl.StringId}}/relationships/fruits' with body: <<{
              "data": [
                {
                  "type": "fruits",
                  "id": "{{existingBanana.StringId}}"
                }
              ]
            }>>
            """,
            $$"""
            [TRACE] Entering PostRelationshipAsync(id: {{existingBowl.StringId}}, relationshipName: "fruits", rightResourceIds: [
              {
                "ClrType": "{{typeof(Fruit).FullName}}",
                "StringId": "{{existingBanana.StringId}}"
              }
            ])
            """,
            $$"""
            [TRACE] Entering AddToToManyRelationshipAsync(leftId: {{existingBowl.StringId}}, relationshipName: "fruits", rightResourceIds: [
              {
                "ClrType": "{{typeof(Fruit).FullName}}",
                "StringId": "{{existingBanana.StringId}}"
              }
            ])
            """,
            $$"""
            [TRACE] Entering GetAsync(queryLayer: QueryLayer<Fruit>
            {
              Filter: equals(id,'{{existingBanana.Id}}')
              Selection
              {
                FieldSelectors<Fruit>
                {
                  id
                }
              }
            }
            )
            """,
            $$"""
            [TRACE] Entering ApplyQueryLayer(queryLayer: QueryLayer<Fruit>
            {
              Filter: equals(id,'{{existingBanana.Id}}')
              Selection
              {
                FieldSelectors<Fruit>
                {
                  id
                }
              }
            }
            )
            """,
            $$"""
            [TRACE] Entering AddToToManyRelationshipAsync(leftResource: null, leftId: {{existingBowl.Id}}, rightResourceIds: [
              {
                "Color": "Yellow",
                "LengthInCentimeters": {{existingBanana.LengthInCentimeters.ToString(CultureInfo.InvariantCulture)}},
                "WeightInKilograms": {{existingBanana.WeightInKilograms.ToString(CultureInfo.InvariantCulture)}},
                "Id": {{existingBanana.Id}},
                "StringId": "{{existingBanana.StringId}}"
              }
            ])
            """
        }, options => options.Using(IgnoreLineEndingsComparer.Instance).WithStrictOrdering());
    }

    [Fact]
    public async Task Logs_method_parameters_of_concrete_resource_type_at_Trace_level()
    {
        // Arrange
        var loggerProvider = _testContext.Factory.Services.GetRequiredService<CapturingLoggerProvider>();
        loggerProvider.Clear();

        var existingBowl = new FruitBowl();
        Peach existingPeach = _fakers.Peach.GenerateOne();

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

        string[] traceLines = loggerProvider.GetMessages().Where(message => message.LogLevel == LogLevel.Trace).Select(message => message.ToString()).ToArray();

        traceLines.Should().BeEquivalentTo(new[]
        {
            $$"""
            [TRACE] Received POST request at 'http://localhost/fruitBowls/{{existingBowl.StringId}}/relationships/fruits' with body: <<{
              "data": [
                {
                  "type": "peaches",
                  "id": "{{existingPeach.StringId}}"
                }
              ]
            }>>
            """,
            $$"""
            [TRACE] Entering PostRelationshipAsync(id: {{existingBowl.StringId}}, relationshipName: "fruits", rightResourceIds: [
              {
                "Color": "Red/Yellow",
                "DiameterInCentimeters": 0,
                "WeightInKilograms": 0,
                "Id": {{existingPeach.Id}},
                "StringId": "{{existingPeach.StringId}}"
              }
            ])
            """,
            $$"""
            [TRACE] Entering AddToToManyRelationshipAsync(leftId: {{existingBowl.StringId}}, relationshipName: "fruits", rightResourceIds: [
              {
                "Color": "Red/Yellow",
                "DiameterInCentimeters": 0,
                "WeightInKilograms": 0,
                "Id": {{existingPeach.Id}},
                "StringId": "{{existingPeach.StringId}}"
              }
            ])
            """,
            $$"""
            [TRACE] Entering GetAsync(queryLayer: QueryLayer<Fruit>
            {
              Filter: equals(id,'{{existingPeach.Id}}')
              Selection
              {
                FieldSelectors<Fruit>
                {
                  id
                }
              }
            }
            )
            """,
            $$"""
            [TRACE] Entering ApplyQueryLayer(queryLayer: QueryLayer<Fruit>
            {
              Filter: equals(id,'{{existingPeach.Id}}')
              Selection
              {
                FieldSelectors<Fruit>
                {
                  id
                }
              }
            }
            )
            """,
            $$"""
            [TRACE] Entering AddToToManyRelationshipAsync(leftResource: null, leftId: {{existingBowl.Id}}, rightResourceIds: [
              {
                "Color": "Red/Yellow",
                "DiameterInCentimeters": 0,
                "WeightInKilograms": 0,
                "Id": {{existingPeach.Id}},
                "StringId": "{{existingPeach.StringId}}"
              }
            ])
            """
        }, options => options.Using(IgnoreLineEndingsComparer.Instance).WithStrictOrdering());
    }

    [Fact]
    public async Task Logs_query_layer_and_expression_at_Debug_level()
    {
        // Arrange
        var loggerProvider = _testContext.Factory.Services.GetRequiredService<CapturingLoggerProvider>();
        loggerProvider.Clear();

        var bowl = new FruitBowl();
        bowl.Fruits.Add(_fakers.Peach.GenerateOne());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.FruitBowls.Add(bowl);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/fruitBowls/{bowl.StringId}/fruits?filter=greaterThan(weightInKilograms,'0.1')&fields[peaches]=color&sort=-id";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteGetAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Should().NotBeEmpty();

        LogMessage queryLayerMessage = loggerProvider.GetMessages().Should()
            .ContainSingle(message => message.LogLevel == LogLevel.Debug && message.Text.StartsWith("QueryLayer:", StringComparison.Ordinal)).Subject;

        queryLayerMessage.Text.Should().Be($$"""
            QueryLayer: QueryLayer<FruitBowl>
            {
              Include: fruits
              Filter: equals(id,'{{bowl.StringId}}')
              Selection
              {
                FieldSelectors<FruitBowl>
                {
                  id
                  fruits: QueryLayer<Fruit>
                  {
                    Filter: greaterThan(weightInKilograms,'0.1')
                    Sort: -id
                    Pagination: Page number: 1, size: 10
                    Selection
                    {
                      FieldSelectors<Peach>
                      {
                        color
                        id
                      }
                    }
                  }
                }
              }
            }

            """);

        LogMessage expressionMessage = loggerProvider.GetMessages().Should().ContainSingle(message =>
            message.LogLevel == LogLevel.Debug && message.Text.StartsWith("Expression tree:", StringComparison.Ordinal)).Subject;

        expressionMessage.Text.Should().Be("""
            Expression tree: [Microsoft.EntityFrameworkCore.Query.EntityQueryRootExpression]
                .AsNoTrackingWithIdentityResolution()
                .Include("Fruits")
                .Where(fruitBowl => fruitBowl.Id == value)
                .Select(
                    fruitBowl => new FruitBowl
                    {
                        Id = fruitBowl.Id,
                        Fruits = fruitBowl.Fruits
                            .Where(fruit => fruit.WeightInKilograms > value)
                            .OrderByDescending(fruit => fruit.Id)
                            .Take(value)
                            .Select(
                                fruit => (fruit.GetType() == value)
                                    ? (Fruit)new Peach
                                    {
                                        Id = fruit.Id,
                                        WeightInKilograms = fruit.WeightInKilograms,
                                        DiameterInCentimeters = ((Peach)fruit).DiameterInCentimeters,
                                        Id = fruit.Id
                                    }
                                    : fruit)
                            .ToHashSet()
                    })
            """);
    }
}
