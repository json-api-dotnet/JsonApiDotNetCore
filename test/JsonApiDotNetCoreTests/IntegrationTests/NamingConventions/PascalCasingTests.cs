using System.Net;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.NamingConventions;

public sealed class PascalCasingTests : IClassFixture<IntegrationTestContext<PascalCasingConventionStartup<NamingDbContext>, NamingDbContext>>
{
    private readonly IntegrationTestContext<PascalCasingConventionStartup<NamingDbContext>, NamingDbContext> _testContext;
    private readonly NamingFakers _fakers = new();

    public PascalCasingTests(IntegrationTestContext<PascalCasingConventionStartup<NamingDbContext>, NamingDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<DivingBoardsController>();
        testContext.UseController<SwimmingPoolsController>();
    }

    [Fact]
    public async Task Can_get_resources_with_include()
    {
        // Arrange
        List<SwimmingPool> pools = _fakers.SwimmingPool.GenerateList(2);
        pools[1].DivingBoards = _fakers.DivingBoard.GenerateList(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<SwimmingPool>();
            dbContext.SwimmingPools.AddRange(pools);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/PublicApi/SwimmingPools?include=DivingBoards";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(2);
        responseDocument.Data.ManyValue.Should().OnlyContain(resource => resource.Type == "SwimmingPools");
        responseDocument.Data.ManyValue.Should().OnlyContain(resource => resource.Attributes.Should().ContainKey2("IsIndoor").WhoseValue != null);
        responseDocument.Data.ManyValue.Should().OnlyContain(resource => resource.Relationships.Should().ContainKey2("WaterSlides").WhoseValue != null);
        responseDocument.Data.ManyValue.Should().OnlyContain(resource => resource.Relationships.Should().ContainKey2("DivingBoards").WhoseValue != null);

        decimal height = pools[1].DivingBoards[0].HeightInMeters;
        string link = $"/PublicApi/DivingBoards/{pools[1].DivingBoards[0].StringId}";

        responseDocument.Included.Should().HaveCount(1);
        responseDocument.Included[0].Type.Should().Be("DivingBoards");
        responseDocument.Included[0].Id.Should().Be(pools[1].DivingBoards[0].StringId);
        responseDocument.Included[0].Attributes.Should().ContainKey("HeightInMeters").WhoseValue.As<decimal>().Should().BeApproximately(height);
        responseDocument.Included[0].Relationships.Should().BeNull();
        responseDocument.Included[0].Links.RefShould().NotBeNull().And.Subject.Self.Should().Be(link);

        responseDocument.Meta.Should().ContainTotal(2, "Total");
    }

    [Fact]
    public async Task Can_filter_secondary_resources_with_sparse_fieldset()
    {
        // Arrange
        SwimmingPool pool = _fakers.SwimmingPool.GenerateOne();
        pool.WaterSlides = _fakers.WaterSlide.GenerateList(2);
        pool.WaterSlides[0].LengthInMeters = 1;
        pool.WaterSlides[1].LengthInMeters = 5;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.SwimmingPools.Add(pool);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/PublicApi/SwimmingPools/{pool.StringId}/WaterSlides?filter=greaterThan(LengthInMeters,'1')&fields[WaterSlides]=LengthInMeters";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Type.Should().Be("WaterSlides");
        responseDocument.Data.ManyValue[0].Id.Should().Be(pool.WaterSlides[1].StringId);
        responseDocument.Data.ManyValue[0].Attributes.Should().HaveCount(1);
    }

    [Fact]
    public async Task Can_create_resource()
    {
        // Arrange
        SwimmingPool newPool = _fakers.SwimmingPool.GenerateOne();

        var requestBody = new
        {
            data = new
            {
                type = "SwimmingPools",
                attributes = new Dictionary<string, object>
                {
                    ["IsIndoor"] = newPool.IsIndoor
                }
            }
        };

        const string route = "/PublicApi/SwimmingPools";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.Created);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Type.Should().Be("SwimmingPools");
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("IsIndoor").WhoseValue.Should().Be(newPool.IsIndoor);

        int newPoolId = int.Parse(responseDocument.Data.SingleValue.Id.Should().NotBeNull().And.Subject);
        string poolLink = $"{route}/{newPoolId}";

        responseDocument.Data.SingleValue.Relationships.Should().ContainKey("WaterSlides").WhoseValue.With(value =>
        {
            value.Should().NotBeNull();
            value.Links.Should().NotBeNull();
            value.Links.Self.Should().Be($"{poolLink}/relationships/WaterSlides");
            value.Links.Related.Should().Be($"{poolLink}/WaterSlides");
        });

        responseDocument.Data.SingleValue.Relationships.Should().ContainKey("DivingBoards").WhoseValue.With(value =>
        {
            value.Should().NotBeNull();
            value.Links.Should().NotBeNull();
            value.Links.Self.Should().Be($"{poolLink}/relationships/DivingBoards");
            value.Links.Related.Should().Be($"{poolLink}/DivingBoards");
        });

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            SwimmingPool poolInDatabase = await dbContext.SwimmingPools.FirstWithIdAsync(newPoolId);

            poolInDatabase.IsIndoor.Should().Be(newPool.IsIndoor);
        });
    }

    [Fact]
    public async Task Applies_casing_convention_on_error_stack_trace()
    {
        // Arrange
        const string requestBody = "{ \"data\": {";

        const string route = "/PublicApi/SwimmingPools";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Failed to deserialize request body.");
        error.Meta.Should().ContainKey("StackTrace");
    }

    [Fact]
    public async Task Applies_casing_convention_on_source_pointer_from_ModelState()
    {
        // Arrange
        DivingBoard existingBoard = _fakers.DivingBoard.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.DivingBoards.Add(existingBoard);
            await dbContext.SaveChangesAsync();
        });

        var requestBody = new
        {
            data = new
            {
                type = "DivingBoards",
                id = existingBoard.StringId,
                attributes = new Dictionary<string, object>
                {
                    ["HeightInMeters"] = -1
                }
            }
        };

        string route = $"/PublicApi/DivingBoards/{existingBoard.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.UnprocessableEntity);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        error.Title.Should().Be("Input validation failed.");
        error.Detail.Should().Be("The field HeightInMeters must be between 1 and 20.");
        error.Source.Should().NotBeNull();
        error.Source.Pointer.Should().Be("/data/attributes/HeightInMeters");
    }
}
