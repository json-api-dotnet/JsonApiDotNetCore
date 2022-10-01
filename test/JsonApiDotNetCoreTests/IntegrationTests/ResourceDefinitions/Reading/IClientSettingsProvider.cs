namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading;

public interface IClientSettingsProvider
{
    bool IsIncludePlanetMoonsBlocked { get; }
    bool ArePlanetsWithPrivateNameHidden { get; }
    bool IsStarGivingLightToMoonAutoIncluded { get; }
}
