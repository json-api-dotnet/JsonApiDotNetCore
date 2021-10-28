using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Serialization.Objects;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.NamingConventions
{
    public sealed class KebabCasingTests : IClassFixture<IntegrationTestContext<KebabCasingConventionStartup<NamingDbContext>, NamingDbContext>>
    {
        private readonly IntegrationTestContext<KebabCasingConventionStartup<NamingDbContext>, NamingDbContext> _testContext;
        private readonly NamingFakers _fakers = new();

        public KebabCasingTests(IntegrationTestContext<KebabCasingConventionStartup<NamingDbContext>, NamingDbContext> testContext)
        {
            _testContext = testContext;

            testContext.UseController<DivingBoardsController>();
            testContext.UseController<SwimmingPoolsController>();
        }

        [Fact]
        public async Task Can_get_resources_with_include()
        {
            // Arrange
            List<SwimmingPool> pools = _fakers.SwimmingPool.Generate(2);
            pools[1].DivingBoards = _fakers.DivingBoard.Generate(1);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<SwimmingPool>();
                dbContext.SwimmingPools.AddRange(pools);
                await dbContext.SaveChangesAsync();
            });

            const string route = "/public-api/swimming-pools?include=diving-boards";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.ManyValue.ShouldHaveCount(2);
            responseDocument.Data.ManyValue.Should().OnlyContain(resourceObject => resourceObject.Type == "swimming-pools");
            responseDocument.Data.ManyValue.Should().OnlyContain(resourceObject => resourceObject.Attributes.ShouldContainKey("is-indoor") != null);
            responseDocument.Data.ManyValue.Should().OnlyContain(resourceObject => resourceObject.Relationships.ShouldContainKey("water-slides") != null);
            responseDocument.Data.ManyValue.Should().OnlyContain(resourceObject => resourceObject.Relationships.ShouldContainKey("diving-boards") != null);

            decimal height = pools[1].DivingBoards[0].HeightInMeters;

            responseDocument.Included.ShouldHaveCount(1);
            responseDocument.Included[0].Type.Should().Be("diving-boards");
            responseDocument.Included[0].Id.Should().Be(pools[1].DivingBoards[0].StringId);
            responseDocument.Included[0].Attributes.ShouldContainKey("height-in-meters").With(value => value.As<decimal>().Should().BeApproximately(height));
            responseDocument.Included[0].Relationships.Should().BeNull();
            responseDocument.Included[0].Links.ShouldNotBeNull().Self.Should().Be($"/public-api/diving-boards/{pools[1].DivingBoards[0].StringId}");

            responseDocument.Meta.ShouldContainKey("total").With(value =>
            {
                JsonElement element = value.Should().BeOfType<JsonElement>().Subject;
                element.GetInt32().Should().Be(2);
            });
        }

        [Fact]
        public async Task Can_filter_secondary_resources_with_sparse_fieldset()
        {
            // Arrange
            SwimmingPool pool = _fakers.SwimmingPool.Generate();
            pool.WaterSlides = _fakers.WaterSlide.Generate(2);
            pool.WaterSlides[0].LengthInMeters = 1;
            pool.WaterSlides[1].LengthInMeters = 5;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.SwimmingPools.Add(pool);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/public-api/swimming-pools/{pool.StringId}/water-slides" +
                "?filter=greaterThan(length-in-meters,'1')&fields[water-slides]=length-in-meters";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.Data.ManyValue.ShouldHaveCount(1);
            responseDocument.Data.ManyValue[0].Type.Should().Be("water-slides");
            responseDocument.Data.ManyValue[0].Id.Should().Be(pool.WaterSlides[1].StringId);
            responseDocument.Data.ManyValue[0].Attributes.ShouldHaveCount(1);
        }

        [Fact]
        public async Task Can_create_resource()
        {
            // Arrange
            SwimmingPool newPool = _fakers.SwimmingPool.Generate();

            var requestBody = new
            {
                data = new
                {
                    type = "swimming-pools",
                    attributes = new Dictionary<string, object>
                    {
                        ["is-indoor"] = newPool.IsIndoor
                    }
                }
            };

            const string route = "/public-api/swimming-pools";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.Created);

            responseDocument.Data.SingleValue.ShouldNotBeNull();
            responseDocument.Data.SingleValue.Type.Should().Be("swimming-pools");
            responseDocument.Data.SingleValue.Attributes.ShouldContainKey("is-indoor").With(value => value.Should().Be(newPool.IsIndoor));

            int newPoolId = int.Parse(responseDocument.Data.SingleValue.Id.ShouldNotBeNull());
            string poolLink = $"{route}/{newPoolId}";

            responseDocument.Data.SingleValue.Relationships.ShouldContainKey("water-slides").With(value =>
            {
                value.ShouldNotBeNull();
                value.Links.ShouldNotBeNull();
                value.Links.Self.Should().Be($"{poolLink}/relationships/water-slides");
                value.Links.Related.Should().Be($"{poolLink}/water-slides");
            });

            responseDocument.Data.SingleValue.Relationships.ShouldContainKey("diving-boards").With(value =>
            {
                value.ShouldNotBeNull();
                value.Links.ShouldNotBeNull();
                value.Links.Self.Should().Be($"{poolLink}/relationships/diving-boards");
                value.Links.Related.Should().Be($"{poolLink}/diving-boards");
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

            const string route = "/public-api/swimming-pools";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePostAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.ShouldHaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error.Title.Should().Be("Failed to deserialize request body.");
            error.Meta.ShouldContainKey("stack-trace");
        }

        [Fact]
        public async Task Applies_casing_convention_on_source_pointer_from_ModelState()
        {
            // Arrange
            DivingBoard existingBoard = _fakers.DivingBoard.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.DivingBoards.Add(existingBoard);
                await dbContext.SaveChangesAsync();
            });

            var requestBody = new
            {
                data = new
                {
                    type = "diving-boards",
                    id = existingBoard.StringId,
                    attributes = new Dictionary<string, object>
                    {
                        ["height-in-meters"] = -1
                    }
                }
            };

            string route = $"/public-api/diving-boards/{existingBoard.StringId}";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecutePatchAsync<Document>(route, requestBody);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.UnprocessableEntity);

            responseDocument.Errors.ShouldHaveCount(1);

            ErrorObject error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            error.Title.Should().Be("Input validation failed.");
            error.Detail.Should().Be("The field HeightInMeters must be between 1 and 20.");
            error.Source.ShouldNotBeNull();
            error.Source.Pointer.Should().Be("/data/attributes/height-in-meters");
        }
    }
}
