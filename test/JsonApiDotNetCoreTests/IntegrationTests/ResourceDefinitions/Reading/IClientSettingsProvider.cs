namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading;

public interface IClientSettingsProvider
{
    bool AreVeryLargeStarsHidden { get; }
    bool AreConstellationsVisibleDuringWinterHidden { get; }
    bool IsIncludePlanetMoonsBlocked { get; }
    bool ArePlanetsWithPrivateNameHidden { get; }
    bool IsStarGivingLightToMoonAutoIncluded { get; }
    PlanetFieldSetBehavior PlanetFieldSetBehavior { get; }
    bool IsPlanetPageSizeLimited { get; }
    bool AreSmallMoonsHidden { get; }
    bool IsConstellationStarsIncludeRewritten { get; }
}
