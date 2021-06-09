using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using JsonApiDotNetCoreExampleTests.Startups;
using Microsoft.Extensions.DependencyInjection;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceDefinitions.Reading
{
    public sealed class ResourceDefinitionReadTests : IClassFixture<ExampleIntegrationTestContext<TestableStartup<UniverseDbContext>, UniverseDbContext>>
    {
        private readonly ExampleIntegrationTestContext<TestableStartup<UniverseDbContext>, UniverseDbContext> _testContext;
        private readonly UniverseFakers _fakers = new UniverseFakers();

        public ResourceDefinitionReadTests(ExampleIntegrationTestContext<TestableStartup<UniverseDbContext>, UniverseDbContext> testContext)
        {
            _testContext = testContext;

            testContext.UseController<StarsController>();
            testContext.UseController<PlanetsController>();
            testContext.UseController<MoonsController>();

            testContext.ConfigureServicesAfterStartup(services =>
            {
                services.AddResourceDefinition<StarDefinition>();
                services.AddResourceDefinition<PlanetDefinition>();
                services.AddResourceDefinition<MoonDefinition>();

                services.AddSingleton<IClientSettingsProvider, TestClientSettingsProvider>();
            });

            var options = (JsonApiOptions)testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.IncludeTotalResourceCount = true;

            var settingsProvider = (TestClientSettingsProvider)testContext.Factory.Services.GetRequiredService<IClientSettingsProvider>();
            settingsProvider.ResetToDefaults();
        }

        [Fact]
        public async Task Include_from_resource_definition_is_blocked()
        {
            // Arrange
            var settingsProvider = (TestClientSettingsProvider)_testContext.Factory.Services.GetRequiredService<IClientSettingsProvider>();
            settingsProvider.BlockIncludePlanetMoons();

            Planet planet = _fakers.Planet.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Planet>();
                dbContext.Planets.Add(planet);
                await dbContext.SaveChangesAsync();
            });

            const string route = "/planets?include=moons";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.Title.Should().Be("Including moons is not permitted.");
            error.Detail.Should().BeNull();
        }

        [Fact]
        public async Task Filter_from_resource_definition_is_applied()
        {
            // Arrange
            var settingsProvider = (TestClientSettingsProvider)_testContext.Factory.Services.GetRequiredService<IClientSettingsProvider>();
            settingsProvider.HidePlanetsWithPrivateName();

            List<Planet> planets = _fakers.Planet.Generate(4);
            planets[0].PrivateName = "A";
            planets[2].PrivateName = "B";

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Planet>();
                dbContext.Planets.AddRange(planets);
                await dbContext.SaveChangesAsync();
            });

            const string route = "/planets";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(2);
            responseDocument.ManyData[0].Id.Should().Be(planets[1].StringId);
            responseDocument.ManyData[1].Id.Should().Be(planets[3].StringId);

            responseDocument.Meta["totalResources"].Should().Be(2);
        }

        [Fact]
        public async Task Filter_from_resource_definition_and_query_string_are_applied()
        {
            // Arrange
            var settingsProvider = (TestClientSettingsProvider)_testContext.Factory.Services.GetRequiredService<IClientSettingsProvider>();
            settingsProvider.HidePlanetsWithPrivateName();

            List<Planet> planets = _fakers.Planet.Generate(4);

            planets[0].HasRingSystem = true;
            planets[0].PrivateName = "A";

            planets[1].HasRingSystem = false;
            planets[1].PrivateName = "B";

            planets[2].HasRingSystem = true;

            planets[3].HasRingSystem = false;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Planet>();
                dbContext.Planets.AddRange(planets);
                await dbContext.SaveChangesAsync();
            });

            const string route = "/planets?filter=equals(hasRingSystem,'false')";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(planets[3].StringId);

            responseDocument.Meta["totalResources"].Should().Be(1);
        }

        [Fact]
        public async Task Sort_from_resource_definition_is_applied()
        {
            // Arrange
            List<Star> stars = _fakers.Star.Generate(3);

            stars[0].SolarMass = 500m;
            stars[0].SolarRadius = 1m;

            stars[1].SolarMass = 500m;
            stars[1].SolarRadius = 10m;

            stars[2].SolarMass = 50m;
            stars[2].SolarRadius = 15m;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Star>();
                dbContext.Stars.AddRange(stars);
                await dbContext.SaveChangesAsync();
            });

            const string route = "/stars";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(3);
            responseDocument.ManyData[0].Id.Should().Be(stars[1].StringId);
            responseDocument.ManyData[1].Id.Should().Be(stars[0].StringId);
            responseDocument.ManyData[2].Id.Should().Be(stars[2].StringId);
        }

        [Fact]
        public async Task Sort_from_query_string_is_applied()
        {
            // Arrange
            List<Star> stars = _fakers.Star.Generate(3);

            stars[0].Name = "B";
            stars[0].SolarRadius = 10m;

            stars[1].Name = "B";
            stars[1].SolarRadius = 1m;

            stars[2].Name = "A";
            stars[2].SolarRadius = 15m;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Star>();
                dbContext.Stars.AddRange(stars);
                await dbContext.SaveChangesAsync();
            });

            const string route = "/stars?sort=name,-solarRadius";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(3);
            responseDocument.ManyData[0].Id.Should().Be(stars[2].StringId);
            responseDocument.ManyData[1].Id.Should().Be(stars[0].StringId);
            responseDocument.ManyData[2].Id.Should().Be(stars[1].StringId);
        }

        [Fact]
        public async Task Page_size_from_resource_definition_is_applied()
        {
            // Arrange
            List<Star> stars = _fakers.Star.Generate(10);

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Star>();
                dbContext.Stars.AddRange(stars);
                await dbContext.SaveChangesAsync();
            });

            const string route = "/stars?page[size]=8";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(5);
        }

        [Fact]
        public async Task Attribute_inclusion_from_resource_definition_is_applied_for_omitted_query_string()
        {
            // Arrange
            Star star = _fakers.Star.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Stars.Add(star);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/stars/{star.StringId}";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Id.Should().Be(star.StringId);
            responseDocument.SingleData.Attributes["name"].Should().Be(star.Name);
            responseDocument.SingleData.Attributes["kind"].Should().Be(star.Kind.ToString());
            responseDocument.SingleData.Relationships.Should().NotBeNull();
        }

        [Fact]
        public async Task Attribute_inclusion_from_resource_definition_is_applied_for_fields_query_string()
        {
            // Arrange
            Star star = _fakers.Star.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Stars.Add(star);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/stars/{star.StringId}?fields[stars]=name,solarRadius";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Id.Should().Be(star.StringId);
            responseDocument.SingleData.Attributes.Should().HaveCount(2);
            responseDocument.SingleData.Attributes["name"].Should().Be(star.Name);
            responseDocument.SingleData.Attributes["solarRadius"].As<decimal>().Should().BeApproximately(star.SolarRadius);
            responseDocument.SingleData.Relationships.Should().BeNull();
        }

        [Fact]
        public async Task Attribute_exclusion_from_resource_definition_is_applied_for_omitted_query_string()
        {
            // Arrange
            Star star = _fakers.Star.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Stars.Add(star);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/stars/{star.StringId}";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Id.Should().Be(star.StringId);
            responseDocument.SingleData.Attributes["name"].Should().Be(star.Name);
            responseDocument.SingleData.Attributes.Should().NotContainKey("isVisibleFromEarth");
            responseDocument.SingleData.Relationships.Should().NotBeNull();
        }

        [Fact]
        public async Task Attribute_exclusion_from_resource_definition_is_applied_for_fields_query_string()
        {
            // Arrange
            Star star = _fakers.Star.Generate();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Stars.Add(star);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/stars/{star.StringId}?fields[stars]=name,isVisibleFromEarth";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.SingleData.Id.Should().Be(star.StringId);
            responseDocument.SingleData.Attributes.Should().HaveCount(1);
            responseDocument.SingleData.Attributes["name"].Should().Be(star.Name);
            responseDocument.SingleData.Relationships.Should().BeNull();
        }

        [Fact]
        public async Task Queryable_parameter_handler_from_resource_definition_is_applied()
        {
            // Arrange
            List<Moon> moons = _fakers.Moon.Generate(2);

            moons[0].SolarRadius = .5m;
            moons[1].SolarRadius = 50m;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Moon>();
                dbContext.Moons.AddRange(moons);
                await dbContext.SaveChangesAsync();
            });

            const string route = "/moons?isLargerThanTheSun=true";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(moons[1].StringId);
        }

        [Fact]
        public async Task Queryable_parameter_handler_from_resource_definition_and_query_string_filter_are_applied()
        {
            // Arrange
            List<Moon> moons = _fakers.Moon.Generate(4);

            moons[0].Name = "Alpha1";
            moons[0].SolarRadius = 1m;

            moons[1].Name = "Alpha2";
            moons[1].SolarRadius = 5m;

            moons[2].Name = "Beta1";
            moons[2].SolarRadius = 1m;

            moons[3].Name = "Beta2";
            moons[3].SolarRadius = 5m;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                await dbContext.ClearTableAsync<Moon>();
                dbContext.Moons.AddRange(moons);
                await dbContext.SaveChangesAsync();
            });

            const string route = "/moons?isLargerThanTheSun=false&filter=startsWith(name,'B')";

            // Act
            (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.ManyData.Should().HaveCount(1);
            responseDocument.ManyData[0].Id.Should().Be(moons[2].StringId);
        }

        [Fact]
        public async Task Queryable_parameter_handler_from_resource_definition_is_not_applied_on_secondary_request()
        {
            // Arrange
            Planet planet = _fakers.Planet.Generate();
            planet.Moons = _fakers.Moon.Generate(1).ToHashSet();

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Planets.Add(planet);
                await dbContext.SaveChangesAsync();
            });

            string route = $"/planets/{planet.StringId}/moons?isLargerThanTheSun=false";

            // Act
            (HttpResponseMessage httpResponse, ErrorDocument responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);

            Error error = responseDocument.Errors[0];
            error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            error.Title.Should().Be("Custom query string parameters cannot be used on nested resource endpoints.");
            error.Detail.Should().Be("Query string parameter 'isLargerThanTheSun' cannot be used on a nested resource endpoint.");
            error.Source.Parameter.Should().Be("isLargerThanTheSun");
        }
    }
}
