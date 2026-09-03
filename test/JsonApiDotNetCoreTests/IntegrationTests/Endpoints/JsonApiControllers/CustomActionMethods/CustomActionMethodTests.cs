using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.EntityFrameworkCore;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.Endpoints.JsonApiControllers.CustomActionMethods;

public sealed class CustomActionMethodTests : IClassFixture<IntegrationTestContext<CoffeeStartup, CoffeeDbContext>>
{
    private readonly IntegrationTestContext<CoffeeStartup, CoffeeDbContext> _testContext;
    private readonly CoffeeFakers _fakers = new();

    public CustomActionMethodTests(IntegrationTestContext<CoffeeStartup, CoffeeDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<CupOfCoffeesController>();
        testContext.UseController<CoffeeSummaryController>();
    }

    [Fact]
    public async Task Can_get_coffee_summary()
    {
        // Arrange
        List<CupOfCoffee> cups = _fakers.CupOfCoffee.GenerateList(10);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<CupOfCoffee>();
            dbContext.CupsOfCoffee.AddRange(cups);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/coffeeSummaries/summary";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.RefShould().NotBeNull().And.Subject.With(resource =>
        {
            resource.Attributes.Should().ContainKey("totalCount").WhoseValue.Should().Be(10);
            resource.Attributes.Should().ContainKey("blackCount").WhoseValue.Should().Be(cups.Count(cup => cup is { HasMilk: false, HasSugar: false }));
            resource.Attributes.Should().ContainKey("onlySugarCount").WhoseValue.Should().Be(cups.Count(cup => cup is { HasMilk: false, HasSugar: true }));
            resource.Attributes.Should().ContainKey("onlyMilkCount").WhoseValue.Should().Be(cups.Count(cup => cup is { HasMilk: true, HasSugar: false }));
            resource.Attributes.Should().ContainKey("sugarWithMilkCount").WhoseValue.Should().Be(cups.Count(cup => cup is { HasMilk: true, HasSugar: true }));
        });
    }

    [Fact]
    public async Task Cannot_get_empty_coffee_summary()
    {
        // Arrange
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<CupOfCoffee>();
        });

        const string route = "/coffeeSummaries/summary";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("No cups available to summarize.");
        error.Detail.Should().BeNull();
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Can_get_only_black_cups()
    {
        // Arrange
        List<CupOfCoffee> cups = _fakers.CupOfCoffee.GenerateList(2);
        cups[0].HasSugar = true;
        cups[1].HasMilk = false;
        cups[1].HasSugar = false;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<CupOfCoffee>();
            dbContext.CupsOfCoffee.AddRange(cups);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/cupOfCoffees/onlyBlack";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().ContainSingle().Which.With(resource =>
        {
            resource.Id.Should().Be(cups[1].StringId);
            resource.Attributes.Should().ContainKey("hasMilk").WhoseValue.Should().Be(false);
            resource.Attributes.Should().ContainKey("hasSugar").WhoseValue.Should().Be(false);
        });
    }

    [Fact]
    public async Task Can_get_existing_black_cup()
    {
        // Arrange
        CupOfCoffee cup = _fakers.CupOfCoffee.GenerateOne();
        cup.HasSugar = false;
        cup.HasMilk = false;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.CupsOfCoffee.Add(cup);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/cupOfCoffees/onlyBlack/{cup.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(cup.StringId);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("hasMilk").WhoseValue.Should().Be(false);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("hasSugar").WhoseValue.Should().Be(false);
    }

    [Fact]
    public async Task Cannot_get_unknown_black_cup()
    {
        // Arrange
        string route = $"/cupOfCoffees/onlyBlack/{Unknown.StringId.Int64}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested resource does not exist.");
        error.Detail.Should().Be($"Resource of type 'cupOfCoffees' with ID '{Unknown.StringId.Int64}' does not exist.");
        error.Source.Should().BeNull();
    }

    [Fact]
    public async Task Can_create_cups_in_batch()
    {
        // Arrange
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<CupOfCoffee>();
        });

        var requestBody = new
        {
            data = new
            {
                type = "cupOfCoffees",
                attributes = new
                {
                    hasSugar = true,
                    hasMilk = true
                }
            }
        };

        const string route = "/cupOfCoffees/batch?size=3";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePostAsync<string>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            List<CupOfCoffee> cupsInDatabase = await dbContext.CupsOfCoffee.ToListAsync();

            cupsInDatabase.Should().HaveCount(3);
            cupsInDatabase.Should().AllSatisfy(cup => cup.HasSugar.Should().BeTrue());
            cupsInDatabase.Should().AllSatisfy(cup => cup.HasMilk.Should().BeTrue());
        });
    }

    [Fact]
    public async Task Cannot_create_cups_with_negative_batch_size()
    {
        // Arrange
        var requestBody = new
        {
            data = new
            {
                type = "cupOfCoffees",
                attributes = new
                {
                    hasSugar = true,
                    hasMilk = true
                }
            }
        };

        const string route = "/cupOfCoffees/batch?size=-1";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Invalid batch size.");
        error.Detail.Should().Be("Please specify a batch size of one or higher in the query string.");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("size");
    }

    [Fact]
    public async Task Can_reset_cups_in_batch()
    {
        // Arrange
        List<CupOfCoffee> cups = _fakers.CupOfCoffee.GenerateList(5);
        cups[0].HasSugar = true;
        cups[4].HasSugar = true;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<CupOfCoffee>();
            dbContext.CupsOfCoffee.AddRange(cups);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/cupOfCoffees/batch";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecutePatchAsync<string>(route, new
        {
        });

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            List<CupOfCoffee> cupsInDatabase = await dbContext.CupsOfCoffee.ToListAsync();

            cupsInDatabase.Should().HaveCount(5);
            cupsInDatabase.Should().AllSatisfy(cup => cup.HasSugar.Should().BeFalse());
            cupsInDatabase.Should().AllSatisfy(cup => cup.HasMilk.Should().BeFalse());
        });
    }

    [Fact]
    public async Task Can_delete_all_cups()
    {
        // Arrange
        List<CupOfCoffee> cups = _fakers.CupOfCoffee.GenerateList(2);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<CupOfCoffee>();
            dbContext.CupsOfCoffee.AddRange(cups);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/cupOfCoffees/batch";

        // Act
        (HttpResponseMessage httpResponse, string responseDocument) = await _testContext.ExecuteDeleteAsync<string>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NoContent);

        responseDocument.Should().BeEmpty();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            List<CupOfCoffee> cupsInDatabase = await dbContext.CupsOfCoffee.ToListAsync();

            cupsInDatabase.Should().BeEmpty();
        });
    }

    [Fact]
    public async Task Cannot_delete_all_cups_when_empty()
    {
        // Arrange
        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<CupOfCoffee>();
        });

        const string route = "/cupOfCoffees/batch";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteDeleteAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().BeNull();
        error.Detail.Should().BeNull();
        error.Source.Should().BeNull();
    }
}
