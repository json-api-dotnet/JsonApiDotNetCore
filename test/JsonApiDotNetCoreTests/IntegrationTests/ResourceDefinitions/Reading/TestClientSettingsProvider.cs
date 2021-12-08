namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading;

internal sealed class TestClientSettingsProvider : IClientSettingsProvider
{
    public bool IsIncludePlanetMoonsBlocked { get; private set; }
    public bool ArePlanetsWithPrivateNameHidden { get; private set; }
    public bool IsMoonOrbitingPlanetAutoIncluded { get; private set; }

    public void ResetToDefaults()
    {
        IsIncludePlanetMoonsBlocked = false;
        ArePlanetsWithPrivateNameHidden = false;
        IsMoonOrbitingPlanetAutoIncluded = false;
    }

    public void BlockIncludePlanetMoons()
    {
        IsIncludePlanetMoonsBlocked = true;
    }

    public void HidePlanetsWithPrivateName()
    {
        ArePlanetsWithPrivateNameHidden = true;
    }

    public void AutoIncludeOrbitingPlanetForMoons()
    {
        IsMoonOrbitingPlanetAutoIncluded = true;
    }
}
