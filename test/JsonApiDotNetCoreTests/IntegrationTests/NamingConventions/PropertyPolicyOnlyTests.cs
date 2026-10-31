using System.Net;
using System.Text.Json;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.NamingConventions;

public sealed class PropertyPolicyOnlyTests : IClassFixture<IntegrationTestContext<PropertyPolicyOnlyConventionStartup<NamingDbContext>, NamingDbContext>>
{
    private readonly IntegrationTestContext<PropertyPolicyOnlyConventionStartup<NamingDbContext>, NamingDbContext> _testContext;
    private readonly NamingFakers _fakers = new();

    public PropertyPolicyOnlyTests(IntegrationTestContext<PropertyPolicyOnlyConventionStartup<NamingDbContext>, NamingDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<DivingBoardsController>();
        testContext.UseController<SwimmingPoolsController>();
    }

    [Fact(Skip = "KNOWN-ISSUE D9: nested keys are written using DictionaryKeyPolicy (camelCase by default) instead of following PropertyNamingPolicy")]
    public async Task Does_not_camel_case_nested_keys_when_only_property_policy_is_null()
    {
        // Arrange
        DivingBoard board = _fakers.DivingBoard.GenerateOne();

        board.Schedule = new PoolSchedule
        {
            OpensAt = new TimeOnly(9, 0),
            ClosesAt = new TimeOnly(17, 30),
            IsHeated = true
        };

        board.Reservations = [new LaneReservation { LaneNumber = 2, BookedBy = "Bob" }];

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.DivingBoards.Add(board);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/PublicApi/DivingBoards/{board.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();

        JsonElement schedule = responseDocument.Data.SingleValue.Attributes.Should().ContainKey("Schedule").WhoseValue.Should().BeOfType<JsonElement>().Subject;
        string[] scheduleKeys = schedule.EnumerateObject().Select(property => property.Name).ToArray();
        scheduleKeys.Should().BeEquivalentTo("OpensAt", "ClosesAt", "IsHeated");

        JsonElement reservations = responseDocument.Data.SingleValue.Attributes.Should().ContainKey("Reservations").WhoseValue.Should().BeOfType<JsonElement>().Subject;
        string[] reservationKeys = reservations[0].EnumerateObject().Select(property => property.Name).ToArray();
        reservationKeys.Should().BeEquivalentTo("LaneNumber", "Booked-By");
    }
}
