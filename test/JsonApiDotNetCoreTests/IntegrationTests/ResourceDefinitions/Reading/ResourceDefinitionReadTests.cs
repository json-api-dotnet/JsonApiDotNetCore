using System.Net;
using System.Text.Json;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TestBuildingBlocks;
using Xunit;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading;

public sealed class ResourceDefinitionReadTests : IClassFixture<IntegrationTestContext<TestableStartup<UniverseDbContext>, UniverseDbContext>>
{
    private readonly IntegrationTestContext<TestableStartup<UniverseDbContext>, UniverseDbContext> _testContext;
    private readonly UniverseFakers _fakers = new();

    public static TheoryData<string, string, string> HookInvocationSequenceData =>
        new()
        {
            { "GET", "/stars", StarCollectionHits },
            { "GET", "/stars?fields[stars]=name", StarCollectionHits },
            { "GET", "/stars?filter=equals(name,'Sun')", StarCollectionHits },
            { "GET", "/stars?sort=name", StarCollectionHits },
            { "GET", "/stars?page[size]=2", StarCollectionHits },
            {
                "GET", "/stars?include=planets",
                "Star.OnApplyPagination Star.OnApplyFilter Star.OnApplySort Star.OnApplySparseFieldSet Star.OnApplyIncludes Planet.OnApplyFilter " +
                "Planet.OnApplySort Planet.OnApplyPagination Planet.OnApplySparseFieldSet Planet.OnApplyIncludes Star.OnApplySparseFieldSet Star.GetMeta " +
                "Planet.OnApplySparseFieldSet Planet.GetMeta Planet.GetMeta"
            },
            { "GET", "/stars/{id}", StarSingleHits },
            { "GET", "/stars/{id}?fields[stars]=name", StarSingleHits },
            { "GET", "/stars/{id}/planets", PlanetSecondaryHits },
            { "GET", "/stars/{id}/planets?filter=equals(hasRingSystem,'true')&sort=publicName&page[size]=2&fields[planets]=publicName", PlanetSecondaryHits },
            { "GET", "/stars/{id}/relationships/planets", PlanetRelationshipHits },
            { "GET", "/stars/{id}/relationships/planets?filter=equals(hasRingSystem,'true')&sort=publicName&page[size]=2", PlanetRelationshipHits },
            { "POST", "/stars", "Star.OnApplyFilter Star.OnApplyIncludes Star.OnApplySparseFieldSet Star.GetMeta" },
            { "PATCH", "/stars/{id}", "Star.OnApplyFilter Star.OnApplyFilter Star.OnApplyIncludes" },
            { "DELETE", "/stars/{id}", "" }
        };

    public ResourceDefinitionReadTests(IntegrationTestContext<TestableStartup<UniverseDbContext>, UniverseDbContext> testContext)
    {
        _testContext = testContext;

        testContext.UseController<ConstellationsController>();
        testContext.UseController<StarsController>();
        testContext.UseController<PlanetsController>();
        testContext.UseController<MoonsController>();

        testContext.ConfigureServices(services =>
        {
            services.AddResourceDefinition<ConstellationDefinition>();
            services.AddResourceDefinition<StarDefinition>();
            services.AddResourceDefinition<PlanetDefinition>();
            services.AddResourceDefinition<MoonDefinition>();

            services.AddSingleton<IClientSettingsProvider, TestClientSettingsProvider>();
            services.AddSingleton<ResourceDefinitionHitCounter>();

            services.AddSingleton<SparseFieldSetEvaluatorHitCounter>();
            services.Replace(ServiceDescriptor.Scoped<ISparseFieldSetEvaluator, CountingSparseFieldSetEvaluator>());

            services.AddSingleton<CustomConstraintSettings>();
            services.AddScoped<IQueryConstraintProvider, CustomConstraintProvider>();
        });

        var options = (JsonApiOptions)testContext.App.Services.GetRequiredService<IJsonApiOptions>();
        options.IncludeTotalResourceCount = true;

        var settingsProvider = (TestClientSettingsProvider)testContext.App.Services.GetRequiredService<IClientSettingsProvider>();
        settingsProvider.ResetToDefaults();

        var hitCounter = _testContext.App.Services.GetRequiredService<ResourceDefinitionHitCounter>();
        hitCounter.Reset();

        _testContext.App.Services.GetRequiredService<SparseFieldSetEvaluatorHitCounter>().Reset();
        _testContext.App.Services.GetRequiredService<CustomConstraintSettings>().IsEnabled = false;
    }

    [Fact]
    public async Task Has_total_when_pagination_disabled_at_primary_endpoint()
    {
        // Arrange
        var hitCounter = _testContext.App.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        Constellation constellation = _fakers.Constellation.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Constellation>();
            dbContext.Constellations.Add(constellation);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/constellations?page[size]=0";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Meta.Should().ContainTotal(1);

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(Constellation), ResourceDefinitionExtensibilityPoints.OnApplyPagination),
            (typeof(Constellation), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Constellation), ResourceDefinitionExtensibilityPoints.OnApplySort),
            (typeof(Constellation), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Constellation), ResourceDefinitionExtensibilityPoints.OnApplyIncludes),
            (typeof(Constellation), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Constellation), ResourceDefinitionExtensibilityPoints.GetMeta)
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Include_from_resource_definition_is_blocked()
    {
        // Arrange
        var hitCounter = _testContext.App.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        var settingsProvider = (TestClientSettingsProvider)_testContext.App.Services.GetRequiredService<IClientSettingsProvider>();
        settingsProvider.BlockIncludePlanetMoons();

        Planet planet = _fakers.Planet.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Planet>();
            dbContext.Planets.Add(planet);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/planets?include=moons";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Including moons is not permitted.");
        error.Detail.Should().BeNull();

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplyPagination),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplySort),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplyIncludes)
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Include_from_resource_definition_is_added()
    {
        // Arrange
        var hitCounter = _testContext.App.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        var settingsProvider = (TestClientSettingsProvider)_testContext.App.Services.GetRequiredService<IClientSettingsProvider>();
        settingsProvider.AutoIncludeStarGivingLightToMoon();

        Moon moon = _fakers.Moon.GenerateOne();
        moon.OrbitsAround = _fakers.Planet.GenerateOne();
        moon.IsGivenLightBy = _fakers.Star.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Moons.Add(moon);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/moons/{moon.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();

        responseDocument.Data.SingleValue.Relationships.Should().ContainKey("isGivenLightBy").WhoseValue.With(value =>
        {
            value.Should().NotBeNull();
            value.Data.SingleValue.Should().NotBeNull();
            value.Data.SingleValue.Type.Should().Be("stars");
            value.Data.SingleValue.Id.Should().Be(moon.IsGivenLightBy.StringId);
        });

        responseDocument.Included.Should().HaveCount(1);
        responseDocument.Included[0].Type.Should().Be("stars");
        responseDocument.Included[0].Id.Should().Be(moon.IsGivenLightBy.StringId);
        responseDocument.Included[0].Attributes.Should().ContainKey("name").WhoseValue.Should().Be(moon.IsGivenLightBy.Name);

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.OnApplyIncludes),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyIncludes),
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.GetMeta),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.GetMeta)
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Include_from_included_resource_definition_is_added()
    {
        // Arrange
        var hitCounter = _testContext.App.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        var settingsProvider = (TestClientSettingsProvider)_testContext.App.Services.GetRequiredService<IClientSettingsProvider>();
        settingsProvider.AutoIncludeStarGivingLightToMoon();

        Planet planet = _fakers.Planet.GenerateOne();
        planet.Moons = _fakers.Moon.GenerateSet(1);
        planet.Moons.ElementAt(0).IsGivenLightBy = _fakers.Star.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Planets.Add(planet);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/planets/{planet.StringId}?include=moons";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();

        responseDocument.Included.Should().HaveCount(2);

        responseDocument.Included[0].Type.Should().Be("moons");
        responseDocument.Included[0].Id.Should().Be(planet.Moons.ElementAt(0).StringId);
        responseDocument.Included[0].Attributes.Should().ContainKey("name").WhoseValue.Should().Be(planet.Moons.ElementAt(0).Name);

        responseDocument.Included[1].Type.Should().Be("stars");
        responseDocument.Included[1].Id.Should().Be(planet.Moons.ElementAt(0).IsGivenLightBy!.StringId);
        responseDocument.Included[1].Attributes.Should().ContainKey("name").WhoseValue.Should().Be(planet.Moons.ElementAt(0).IsGivenLightBy!.Name);

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplyIncludes),
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.OnApplySort),
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.OnApplyPagination),
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.OnApplyIncludes),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyIncludes),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.GetMeta),
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.GetMeta),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.GetMeta)
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Filter_from_resource_definition_is_applied()
    {
        // Arrange
        var hitCounter = _testContext.App.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        var settingsProvider = (TestClientSettingsProvider)_testContext.App.Services.GetRequiredService<IClientSettingsProvider>();
        settingsProvider.HidePlanetsWithPrivateName();

        List<Planet> planets = _fakers.Planet.GenerateList(4);
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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(2);
        responseDocument.Data.ManyValue[0].Id.Should().Be(planets[1].StringId);
        responseDocument.Data.ManyValue[1].Id.Should().Be(planets[3].StringId);

        responseDocument.Meta.Should().ContainTotal(2);

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplyPagination),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplySort),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplyIncludes),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.GetMeta),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.GetMeta)
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Filter_from_resource_definition_and_query_string_are_applied()
    {
        // Arrange
        var hitCounter = _testContext.App.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        var settingsProvider = (TestClientSettingsProvider)_testContext.App.Services.GetRequiredService<IClientSettingsProvider>();
        settingsProvider.HidePlanetsWithPrivateName();

        List<Planet> planets = _fakers.Planet.GenerateList(4);

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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(planets[3].StringId);

        responseDocument.Meta.Should().ContainTotal(1);

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplyPagination),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplySort),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplyIncludes),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.GetMeta)
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Filter_from_resource_definition_is_applied_at_secondary_endpoint()
    {
        // Arrange
        var hitCounter = _testContext.App.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        var settingsProvider = (TestClientSettingsProvider)_testContext.App.Services.GetRequiredService<IClientSettingsProvider>();
        settingsProvider.HidePlanetsWithPrivateName();

        Star star = _fakers.Star.GenerateOne();
        star.Planets = _fakers.Planet.GenerateSet(4);
        star.Planets.ElementAt(0).PrivateName = "A";
        star.Planets.ElementAt(2).PrivateName = "B";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Stars.Add(star);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/stars/{star.StringId}/planets";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(2);
        responseDocument.Data.ManyValue[0].Id.Should().Be(star.Planets.ElementAt(1).StringId);
        responseDocument.Data.ManyValue[1].Id.Should().Be(star.Planets.ElementAt(3).StringId);

        responseDocument.Meta.Should().ContainTotal(2);

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplyPagination),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplySort),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplyIncludes),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.GetMeta),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.GetMeta)
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Filter_from_resource_definition_is_applied_at_relationship_endpoint()
    {
        // Arrange
        var hitCounter = _testContext.App.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        var settingsProvider = (TestClientSettingsProvider)_testContext.App.Services.GetRequiredService<IClientSettingsProvider>();
        settingsProvider.HidePlanetsWithPrivateName();

        Star star = _fakers.Star.GenerateOne();
        star.Planets = _fakers.Planet.GenerateSet(4);
        star.Planets.ElementAt(0).PrivateName = "A";
        star.Planets.ElementAt(2).PrivateName = "B";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Stars.Add(star);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/stars/{star.StringId}/relationships/planets";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(2);
        responseDocument.Data.ManyValue[0].Id.Should().Be(star.Planets.ElementAt(1).StringId);
        responseDocument.Data.ManyValue[1].Id.Should().Be(star.Planets.ElementAt(3).StringId);

        responseDocument.Meta.Should().ContainTotal(2);

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplySort),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplyPagination),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplyFilter)
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task No_total_when_resource_definition_has_filter_on_inverse_ManyToOne_at_secondary_endpoint()
    {
        // Arrange
        var hitCounter = _testContext.App.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        var settingsProvider = (TestClientSettingsProvider)_testContext.App.Services.GetRequiredService<IClientSettingsProvider>();
        settingsProvider.HideVeryLargeStars();

        Star star = _fakers.Star.GenerateOne();
        star.Planets = _fakers.Planet.GenerateSet(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Stars.Add(star);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/stars/{star.StringId}/planets";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(star.Planets.ElementAt(0).StringId);

        responseDocument.Meta.Should().NotContainTotal();

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplyPagination),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplySort),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplyIncludes),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.GetMeta)
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Has_total_when_resource_definition_has_filter_on_inverse_ManyToMany_at_secondary_endpoint()
    {
        // Arrange
        var hitCounter = _testContext.App.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        var settingsProvider = (TestClientSettingsProvider)_testContext.App.Services.GetRequiredService<IClientSettingsProvider>();
        settingsProvider.HideConstellationsVisibleDuringWinter();

        Constellation constellation = _fakers.Constellation.GenerateOne();
        constellation.VisibleDuring = Season.Winter;
        constellation.Stars = _fakers.Star.GenerateSet(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Constellations.Add(constellation);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/constellations/{constellation.StringId}/stars";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested resource does not exist.");
        error.Detail.Should().Be($"Resource of type 'constellations' with ID '{constellation.StringId}' does not exist.");

        responseDocument.Meta.Should().ContainTotal(0);

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyPagination),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplySort),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyIncludes),
            (typeof(Constellation), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Constellation), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyFilter)
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task No_total_when_resource_definition_has_filter_on_inverse_ManyToOne_at_relationship_endpoint()
    {
        // Arrange
        var hitCounter = _testContext.App.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        var settingsProvider = (TestClientSettingsProvider)_testContext.App.Services.GetRequiredService<IClientSettingsProvider>();
        settingsProvider.HideVeryLargeStars();

        Star star = _fakers.Star.GenerateOne();
        star.Planets = _fakers.Planet.GenerateSet(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Stars.Add(star);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/stars/{star.StringId}/relationships/planets";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(star.Planets.ElementAt(0).StringId);

        responseDocument.Meta.Should().NotContainTotal();

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplySort),
            (typeof(Planet), ResourceDefinitionExtensibilityPoints.OnApplyPagination),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyFilter)
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Has_total_when_resource_definition_has_filter_on_inverse_ManyToMany_at_relationship_endpoint()
    {
        // Arrange
        var hitCounter = _testContext.App.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        var settingsProvider = (TestClientSettingsProvider)_testContext.App.Services.GetRequiredService<IClientSettingsProvider>();
        settingsProvider.HideConstellationsVisibleDuringWinter();

        Constellation constellation = _fakers.Constellation.GenerateOne();
        constellation.VisibleDuring = Season.Winter;
        constellation.Stars = _fakers.Star.GenerateSet(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Constellations.Add(constellation);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/constellations/{constellation.StringId}/relationships/stars";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.NotFound);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.NotFound);
        error.Title.Should().Be("The requested resource does not exist.");
        error.Detail.Should().Be($"Resource of type 'constellations' with ID '{constellation.StringId}' does not exist.");

        responseDocument.Meta.Should().ContainTotal(0);

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplySort),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyPagination),
            (typeof(Constellation), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Constellation), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyFilter)
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Sort_from_resource_definition_is_applied()
    {
        // Arrange
        var hitCounter = _testContext.App.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        List<Star> stars = _fakers.Star.GenerateList(3);

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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(3);
        responseDocument.Data.ManyValue[0].Id.Should().Be(stars[1].StringId);
        responseDocument.Data.ManyValue[1].Id.Should().Be(stars[0].StringId);
        responseDocument.Data.ManyValue[2].Id.Should().Be(stars[2].StringId);

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyPagination),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplySort),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyIncludes),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.GetMeta),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.GetMeta),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.GetMeta)
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Sort_from_query_string_is_applied()
    {
        // Arrange
        var hitCounter = _testContext.App.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        List<Star> stars = _fakers.Star.GenerateList(3);

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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(3);
        responseDocument.Data.ManyValue[0].Id.Should().Be(stars[2].StringId);
        responseDocument.Data.ManyValue[1].Id.Should().Be(stars[0].StringId);
        responseDocument.Data.ManyValue[2].Id.Should().Be(stars[1].StringId);

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyPagination),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplySort),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyIncludes),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.GetMeta),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.GetMeta),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.GetMeta)
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Page_size_from_resource_definition_is_applied()
    {
        // Arrange
        var hitCounter = _testContext.App.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        List<Star> stars = _fakers.Star.GenerateList(10);

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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(5);

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyPagination),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplySort),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyIncludes),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.GetMeta),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.GetMeta),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.GetMeta),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.GetMeta),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.GetMeta)
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Attribute_inclusion_from_resource_definition_is_applied_for_omitted_query_string()
    {
        // Arrange
        var hitCounter = _testContext.App.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        Star star = _fakers.Star.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Stars.Add(star);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/stars/{star.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(star.StringId);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("name").WhoseValue.Should().Be(star.Name);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("kind").WhoseValue.Should().Be(star.Kind);
        responseDocument.Data.SingleValue.Relationships.Should().NotBeNull();

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyIncludes),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.GetMeta)
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Attribute_inclusion_from_resource_definition_is_applied_for_fields_query_string()
    {
        // Arrange
        var hitCounter = _testContext.App.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        Star star = _fakers.Star.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Stars.Add(star);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/stars/{star.StringId}?fields[stars]=name,solarRadius";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(star.StringId);
        responseDocument.Data.SingleValue.Attributes.Should().HaveCount(2);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("name").WhoseValue.Should().Be(star.Name);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("solarRadius").WhoseValue.Should().Be(star.SolarRadius);
        responseDocument.Data.SingleValue.Relationships.Should().BeNull();

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyIncludes),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.GetMeta)
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Attribute_exclusion_from_resource_definition_is_applied_for_omitted_query_string()
    {
        // Arrange
        var hitCounter = _testContext.App.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        Star star = _fakers.Star.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Stars.Add(star);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/stars/{star.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(star.StringId);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("name").WhoseValue.Should().Be(star.Name);
        responseDocument.Data.SingleValue.Attributes.Should().NotContainKey("isVisibleFromEarth");
        responseDocument.Data.SingleValue.Relationships.Should().NotBeNull();

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyIncludes),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.GetMeta)
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Attribute_exclusion_from_resource_definition_is_applied_for_fields_query_string()
    {
        // Arrange
        var hitCounter = _testContext.App.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        Star star = _fakers.Star.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Stars.Add(star);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/stars/{star.StringId}?fields[stars]=name,isVisibleFromEarth";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Id.Should().Be(star.StringId);
        responseDocument.Data.SingleValue.Attributes.Should().HaveCount(1);
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("name").WhoseValue.Should().Be(star.Name);
        responseDocument.Data.SingleValue.Relationships.Should().BeNull();

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplyIncludes),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Star), ResourceDefinitionExtensibilityPoints.GetMeta)
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Queryable_parameter_handler_from_resource_definition_is_applied()
    {
        // Arrange
        var hitCounter = _testContext.App.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        List<Moon> moons = _fakers.Moon.GenerateList(2);

        moons[0].SolarRadius = .5m;
        moons[0].OrbitsAround = _fakers.Planet.GenerateOne();

        moons[1].SolarRadius = 50m;
        moons[1].OrbitsAround = _fakers.Planet.GenerateOne();

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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(moons[1].StringId);

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.OnRegisterQueryableHandlersForQueryStringParameters),
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.OnRegisterQueryableHandlersForQueryStringParameters),
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.OnApplyPagination),
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.OnApplySort),
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.OnApplyIncludes),
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.GetMeta)
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Queryable_parameter_handler_from_resource_definition_and_query_string_filter_are_applied()
    {
        // Arrange
        var hitCounter = _testContext.App.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        List<Moon> moons = _fakers.Moon.GenerateList(4);

        moons[0].Name = "Alpha1";
        moons[0].SolarRadius = 1m;
        moons[0].OrbitsAround = _fakers.Planet.GenerateOne();

        moons[1].Name = "Alpha2";
        moons[1].SolarRadius = 5m;
        moons[1].OrbitsAround = _fakers.Planet.GenerateOne();

        moons[2].Name = "Beta1";
        moons[2].SolarRadius = 1m;
        moons[2].OrbitsAround = _fakers.Planet.GenerateOne();

        moons[3].Name = "Beta2";
        moons[3].SolarRadius = 5m;
        moons[3].OrbitsAround = _fakers.Planet.GenerateOne();

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
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(moons[2].StringId);

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.OnRegisterQueryableHandlersForQueryStringParameters),
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.OnRegisterQueryableHandlersForQueryStringParameters),
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.OnApplyPagination),
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.OnApplyFilter),
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.OnApplySort),
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.OnApplyIncludes),
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet),
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.GetMeta)
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task Queryable_parameter_handler_from_resource_definition_is_not_applied_on_secondary_request()
    {
        // Arrange
        var hitCounter = _testContext.App.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        Planet planet = _fakers.Planet.GenerateOne();
        planet.Moons = _fakers.Moon.GenerateSet(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            dbContext.Planets.Add(planet);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/planets/{planet.StringId}/moons?isLargerThanTheSun=false";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.BadRequest);

        responseDocument.Errors.Should().HaveCount(1);

        ErrorObject error = responseDocument.Errors[0];
        error.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.Title.Should().Be("Custom query string parameters cannot be used on nested resource endpoints.");
        error.Detail.Should().Be("Query string parameter 'isLargerThanTheSun' cannot be used on a nested resource endpoint.");
        error.Source.Should().NotBeNull();
        error.Source.Parameter.Should().Be("isLargerThanTheSun");

        hitCounter.HitExtensibilityPoints.Should().BeEquivalentTo(new[]
        {
            (typeof(Moon), ResourceDefinitionExtensibilityPoints.OnRegisterQueryableHandlersForQueryStringParameters)
        }, options => options.WithStrictOrdering());
    }

    private const string StarCollectionHits = "Star.OnApplyPagination Star.OnApplyFilter Star.OnApplySort Star.OnApplySparseFieldSet Star.OnApplyIncludes " +
        "Star.OnApplySparseFieldSet Star.GetMeta";

    private const string StarSingleHits = "Star.OnApplyFilter Star.OnApplySparseFieldSet Star.OnApplyIncludes Star.OnApplySparseFieldSet Star.GetMeta";

    private const string PlanetSecondaryHits = "Planet.OnApplyPagination Planet.OnApplyFilter Planet.OnApplySort Planet.OnApplySparseFieldSet " +
        "Planet.OnApplyIncludes Star.OnApplyFilter Star.OnApplyFilter Planet.OnApplyFilter Planet.OnApplySparseFieldSet Planet.GetMeta Planet.GetMeta";

    private const string PlanetRelationshipHits = "Planet.OnApplyFilter Planet.OnApplySort Planet.OnApplyPagination Star.OnApplyFilter Star.OnApplyFilter " +
        "Planet.OnApplyFilter";

    public static TheoryData<string, string, string> HookOrderData => new()
    {
        { "GET", "/stars", StarCollectionHits },
        { "GET", "/stars?fields[stars]=name", StarCollectionHits },
        { "GET", "/stars?filter=equals(name,'Sun')", StarCollectionHits },
        { "GET", "/stars?sort=name", StarCollectionHits },
        { "GET", "/stars?page[size]=2", StarCollectionHits },
        {
            "GET", "/stars?include=planets",
            "Star.OnApplyPagination Star.OnApplyFilter Star.OnApplySort Star.OnApplySparseFieldSet Star.OnApplyIncludes Planet.OnApplyFilter " +
            "Planet.OnApplySort Planet.OnApplyPagination Planet.OnApplySparseFieldSet Planet.OnApplyIncludes Star.OnApplySparseFieldSet Star.GetMeta " +
            "Planet.OnApplySparseFieldSet Planet.GetMeta Planet.GetMeta"
        },
        { "GET", "/stars/{id}", StarSingleHits },
        { "GET", "/stars/{id}?fields[stars]=name", StarSingleHits },
        { "GET", "/stars/{id}/planets", PlanetSecondaryHits },
        { "GET", "/stars/{id}/planets?filter=equals(hasRingSystem,'true')&sort=publicName&page[size]=2&fields[planets]=publicName", PlanetSecondaryHits },
        { "GET", "/stars/{id}/relationships/planets", PlanetRelationshipHits },
        { "GET", "/stars/{id}/relationships/planets?filter=equals(hasRingSystem,'true')&sort=publicName&page[size]=2", PlanetRelationshipHits },
        { "POST", "/stars", "Star.OnApplyFilter Star.OnApplyIncludes Star.OnApplySparseFieldSet Star.GetMeta" },
        { "PATCH", "/stars/{id}", "Star.OnApplyFilter Star.OnApplyFilter Star.OnApplyIncludes" },
        { "DELETE", "/stars/{id}", "" }
    };

    [Theory]
    [MemberData(nameof(HookInvocationSequenceData))]
    public async Task Invokes_hooks_in_current_order_per_request_kind(string method, string routeTemplate, string expectedHits)
    {
        // Arrange
        var hitCounter = _testContext.App.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        Star star = _fakers.Star.GenerateOne();
        star.Name = "Sun";
        star.Planets = _fakers.Planet.GenerateSet(2);

        foreach (Planet planet in star.Planets)
        {
            planet.HasRingSystem = true;
        }

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Star>();
            dbContext.Stars.Add(star);
            await dbContext.SaveChangesAsync();
        });

        string route = routeTemplate.Replace("{id}", star.StringId);

        // Act
        HttpResponseMessage httpResponse;

        switch (method)
        {
            case "GET":
            {
                (httpResponse, _) = await _testContext.ExecuteGetAsync<string>(route);
                break;
            }
            case "POST":
            {
                var requestBody = new
                {
                    data = new
                    {
                        type = "stars",
                        attributes = new
                        {
                            name = "New star",
                            solarRadius = 1,
                            solarMass = 1
                        }
                    }
                };

                (httpResponse, _) = await _testContext.ExecutePostAsync<string>(route, requestBody);
                break;
            }
            case "PATCH":
            {
                var requestBody = new
                {
                    data = new
                    {
                        type = "stars",
                        id = star.StringId,
                        attributes = new
                        {
                            name = "Changed star"
                        }
                    }
                };

                (httpResponse, _) = await _testContext.ExecutePatchAsync<string>(route, requestBody);
                break;
            }
            default:
            {
                (httpResponse, _) = await _testContext.ExecuteDeleteAsync<string>(route);
                break;
            }
        }

        // Assert
        httpResponse.IsSuccessStatusCode.Should().BeTrue();

        string actualHits = string.Join(' ', hitCounter.HitExtensibilityPoints.Select(hit => $"{hit.Item1.Name}.{hit.Item2}"));
        actualHits.Should().Be(expectedHits);
    }

    private static JsonElement GetAttribute(ResourceObject resourceObject, string name)
    {
        resourceObject.Attributes.Should().ContainKey(name);
        return JsonSerializer.SerializeToElement(resourceObject.Attributes[name]);
    }

    private static Planet CreatePlanetWithAtmosphere(Planet planet)
    {
        planet.Atmosphere = new Atmosphere
        {
            Pressure = 1.5M,
            Composition =
            [
                new GasFraction
                {
                    Gas = "Nitrogen",
                    Fraction = 0.78M
                },
                new GasFraction
                {
                    Gas = "Oxygen",
                    Fraction = 0.21M
                }
            ]
        };

        return planet;
    }

    [Fact]
    public async Task Invokes_sparse_fieldset_hook_once_per_type_per_phase_when_type_occurs_in_multiple_layers()
    {
        // Arrange
        var hitCounter = _testContext.App.Services.GetRequiredService<ResourceDefinitionHitCounter>();

        Moon moon = _fakers.Moon.GenerateOne();
        moon.OrbitsAround = _fakers.Planet.GenerateOne();

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Moon>();
            dbContext.Moons.Add(moon);
            await dbContext.SaveChangesAsync();
        });

        // Moon occurs in the top layer and again in the nested layer below its planet.
        const string route = "/moons?include=orbitsAround.moons";

        // Act
        (HttpResponseMessage httpResponse, _) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        string[] sparseFieldSetHits = hitCounter.HitExtensibilityPoints.Where(hit => hit.Item2 == ResourceDefinitionExtensibilityPoints.OnApplySparseFieldSet)
            .Select(hit => hit.Item1.Name).ToArray();

        // One hit per type for the query, and one per type for the serializer.
        sparseFieldSetHits.Count(name => name == nameof(Moon)).Should().Be(2);
        sparseFieldSetHits.Count(name => name == nameof(Planet)).Should().Be(2);
    }
    [Fact]
    public async Task Can_include_nested_chain_in_sparse_fieldset_from_hook()
    {
        // Arrange
        var settingsProvider = (TestClientSettingsProvider)_testContext.App.Services.GetRequiredService<IClientSettingsProvider>();
        settingsProvider.SetPlanetFieldSetBehavior(PlanetFieldSetBehavior.IncludeAtmospherePressure);

        var evaluatorCounter = _testContext.App.Services.GetRequiredService<SparseFieldSetEvaluatorHitCounter>();

        Planet planet = CreatePlanetWithAtmosphere(_fakers.Planet.GenerateOne());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Planet>();
            dbContext.Planets.Add(planet);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/planets/{planet.StringId}?fields[planets]=atmosphere.composition";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        // The hook adds the nested chain to the set that is queried from the database, but the response is limited to what the client requested.
        evaluatorCounter.QueryResults.Should().ContainSingle(result => result.StartsWith("planets:", StringComparison.Ordinal));
        evaluatorCounter.QueryResults.Single(result => result.StartsWith("planets:", StringComparison.Ordinal)).Should().Contain("pressure");

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("atmosphere").And.HaveCount(1);

        JsonElement atmosphere = GetAttribute(responseDocument.Data.SingleValue, "atmosphere");
        atmosphere.TryGetProperty("composition", out _).Should().BeTrue();
        atmosphere.TryGetProperty("pressure", out _).Should().BeFalse();
    }

    [Fact]
    public async Task Can_exclude_nested_chain_in_sparse_fieldset_from_hook()
    {
        // Arrange
        var settingsProvider = (TestClientSettingsProvider)_testContext.App.Services.GetRequiredService<IClientSettingsProvider>();
        settingsProvider.SetPlanetFieldSetBehavior(PlanetFieldSetBehavior.ExcludeAtmosphereComposition);

        Planet planet = CreatePlanetWithAtmosphere(_fakers.Planet.GenerateOne());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Planet>();
            dbContext.Planets.Add(planet);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/planets/{planet.StringId}";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("publicName");
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("hasRingSystem");

        JsonElement atmosphere = GetAttribute(responseDocument.Data.SingleValue, "atmosphere");
        atmosphere.TryGetProperty("pressure", out _).Should().BeTrue();
        atmosphere.TryGetProperty("composition", out _).Should().BeFalse();
    }

    [Fact]
    public async Task Intersects_query_string_fieldset_with_hook_result()
    {
        // Arrange
        var settingsProvider = (TestClientSettingsProvider)_testContext.App.Services.GetRequiredService<IClientSettingsProvider>();
        settingsProvider.SetPlanetFieldSetBehavior(PlanetFieldSetBehavior.ExcludeAtmosphereComposition);

        Planet planet = CreatePlanetWithAtmosphere(_fakers.Planet.GenerateOne());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Planet>();
            dbContext.Planets.Add(planet);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/planets/{planet.StringId}?fields[planets]=atmosphere.pressure,atmosphere.composition";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("atmosphere").And.HaveCount(1);

        JsonElement atmosphere = GetAttribute(responseDocument.Data.SingleValue, "atmosphere");
        atmosphere.TryGetProperty("pressure", out _).Should().BeTrue();
        atmosphere.TryGetProperty("composition", out _).Should().BeFalse();
    }

    [Fact]
    public async Task Ignores_query_string_fieldset_when_hook_returns_null()
    {
        // Arrange
        var settingsProvider = (TestClientSettingsProvider)_testContext.App.Services.GetRequiredService<IClientSettingsProvider>();
        settingsProvider.SetPlanetFieldSetBehavior(PlanetFieldSetBehavior.ReturnNull);

        Planet planet = CreatePlanetWithAtmosphere(_fakers.Planet.GenerateOne());

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Planet>();
            dbContext.Planets.Add(planet);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/planets/{planet.StringId}?fields[planets]=publicName";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        // Documents the current rule: a hook that returns null results in the full set of viewable fields being serialized.
        responseDocument.Data.SingleValue.Should().NotBeNull();
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("publicName");
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("hasRingSystem");
        responseDocument.Data.SingleValue.Attributes.Should().ContainKey("atmosphere");
    }

    [Fact]
    public async Task Applies_definition_filter_to_included_collection_with_compound_top()
    {
        // Arrange
        var settingsProvider = (TestClientSettingsProvider)_testContext.App.Services.GetRequiredService<IClientSettingsProvider>();
        settingsProvider.HidePlanetsWithPrivateName();

        Star star = _fakers.Star.GenerateOne();
        star.Classification = new StarClassification
        {
            SpectralClass = "G",
            LuminosityClass = "V"
        };

        star.Planets = _fakers.Planet.GenerateSet(3);
        star.Planets.ElementAt(0).PrivateName = "A";
        star.Planets.ElementAt(2).PrivateName = "B";

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Star>();
            dbContext.Stars.Add(star);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/stars?include=planets";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Relationships.Should().ContainKey("planets");

        responseDocument.Included.Should().HaveCount(1);
        responseDocument.Included[0].Id.Should().Be(star.Planets.ElementAt(1).StringId);
    }

    [Fact]
    public async Task Applies_definition_filter_to_nested_include_chain()
    {
        // Arrange
        var settingsProvider = (TestClientSettingsProvider)_testContext.App.Services.GetRequiredService<IClientSettingsProvider>();
        settingsProvider.HideSmallMoons();

        Star star = _fakers.Star.GenerateOne();
        star.Planets = _fakers.Planet.GenerateSet(1);
        star.Planets.ElementAt(0).Moons = _fakers.Moon.GenerateSet(3);
        star.Planets.ElementAt(0).Moons.ElementAt(0).SolarRadius = 0.5M;
        star.Planets.ElementAt(0).Moons.ElementAt(1).SolarRadius = 2M;
        star.Planets.ElementAt(0).Moons.ElementAt(2).SolarRadius = 3M;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Star>();
            dbContext.Stars.Add(star);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/stars?include=planets.moons";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Included.Should().HaveCount(3);
        responseDocument.Included.Where(resource => resource.Type == "planets").Should().HaveCount(1);

        string[] moonIds = responseDocument.Included.Where(resource => resource.Type == "moons").Select(resource => resource.Id!).ToArray();
        moonIds.Should().BeEquivalentTo(star.Planets.ElementAt(0).Moons.ElementAt(1).StringId, star.Planets.ElementAt(0).Moons.ElementAt(2).StringId);
    }

    [Fact]
    public async Task Applies_definition_pagination_to_included_collection()
    {
        // Arrange
        var settingsProvider = (TestClientSettingsProvider)_testContext.App.Services.GetRequiredService<IClientSettingsProvider>();
        settingsProvider.LimitPlanetPageSize();

        Star star = _fakers.Star.GenerateOne();
        star.Planets = _fakers.Planet.GenerateSet(4);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Star>();
            dbContext.Stars.Add(star);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/stars?include=planets";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Included.Should().HaveCount(2);
    }

    [Fact]
    public async Task Applies_OnApplyIncludes_with_compound()
    {
        // Arrange
        var settingsProvider = (TestClientSettingsProvider)_testContext.App.Services.GetRequiredService<IClientSettingsProvider>();
        settingsProvider.RewriteConstellationStarsInclude();

        Constellation constellation = _fakers.Constellation.GenerateOne();
        constellation.Origin = new ConstellationOrigin
        {
            Culture = "Greek",
            CatalogedInCentury = 2
        };

        constellation.Stars = _fakers.Star.GenerateSet(1);
        constellation.Stars.ElementAt(0).Planets = _fakers.Planet.GenerateSet(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Constellation>();
            dbContext.Constellations.Add(constellation);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/constellations?include=stars";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Included.Should().HaveCount(2);
        responseDocument.Included.Should().ContainSingle(resource => resource.Type == "stars");
        responseDocument.Included.Should().ContainSingle(resource => resource.Type == "planets");

        JsonElement origin = GetAttribute(responseDocument.Data.ManyValue![0], "origin");
        origin.GetProperty("culture").GetString().Should().Be("Greek");
    }

    [Fact]
    public async Task Calls_replaced_sparse_fieldset_evaluator()
    {
        // Arrange
        var evaluatorCounter = _testContext.App.Services.GetRequiredService<SparseFieldSetEvaluatorHitCounter>();

        Star star = _fakers.Star.GenerateOne();
        star.Planets = _fakers.Planet.GenerateSet(1);

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Star>();
            dbContext.Stars.Add(star);
            await dbContext.SaveChangesAsync();
        });

        string route = $"/stars/{star.StringId}?include=planets";

        // Act
        (HttpResponseMessage httpResponse, Document _) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        evaluatorCounter.Calls.Should().Equal("stars:ForQuery", "planets:ForQuery", "Reset", "stars:ForSerializer", "planets:ForSerializer", "stars:ForSerializer",
            "planets:ForSerializer");
    }

    [Fact]
    public async Task Applies_constraints_from_custom_provider_of_both_scoped_kinds()
    {
        // Arrange
        var constraintSettings = _testContext.App.Services.GetRequiredService<CustomConstraintSettings>();
        constraintSettings.IsEnabled = true;

        List<Star> stars = _fakers.Star.GenerateList(2);
        stars[0].IsVisibleFromEarth = true;
        stars[0].Planets = _fakers.Planet.GenerateSet(2);
        stars[0].Planets.ElementAt(0).HasRingSystem = true;
        stars[0].Planets.ElementAt(1).HasRingSystem = false;
        stars[1].IsVisibleFromEarth = false;

        await _testContext.RunOnDatabaseAsync(async dbContext =>
        {
            await dbContext.ClearTableAsync<Star>();
            dbContext.Stars.AddRange(stars);
            await dbContext.SaveChangesAsync();
        });

        const string route = "/stars?include=planets";

        // Act
        (HttpResponseMessage httpResponse, Document responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

        // Assert
        httpResponse.ShouldHaveStatusCode(HttpStatusCode.OK);

        responseDocument.Data.ManyValue.Should().HaveCount(1);
        responseDocument.Data.ManyValue[0].Id.Should().Be(stars[0].StringId);

        responseDocument.Included.Should().HaveCount(1);
        responseDocument.Included[0].Id.Should().Be(stars[0].Planets.ElementAt(0).StringId);
        responseDocument.Included[0].Attributes.Should().ContainKey("publicName").And.HaveCount(1);
    }
}
