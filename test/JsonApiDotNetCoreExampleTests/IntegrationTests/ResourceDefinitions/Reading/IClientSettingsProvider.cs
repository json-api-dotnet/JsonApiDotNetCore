namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceDefinitions.Reading
{
    public interface IClientSettingsProvider
    {
        bool IsIncludePlanetMoonsBlocked { get; }
        bool ArePlanetsWithPrivateNameHidden { get; }
        bool IsMoonOrbitingPlanetAutoIncluded { get; }
    }
}
