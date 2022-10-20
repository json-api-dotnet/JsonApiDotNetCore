namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading;

internal sealed class TestClientSettingsProvider : IClientSettingsProvider
{
    public bool IsIncludePlanetMoonsBlocked { get; private set; }
    public bool ArePlanetsWithPrivateNameHidden { get; private set; }
    public bool IsStarGivingLightToMoonAutoIncluded { get; private set; }

    public void ResetToDefaults()
    {
        IsIncludePlanetMoonsBlocked = false;
        ArePlanetsWithPrivateNameHidden = false;
        IsStarGivingLightToMoonAutoIncluded = false;
    }

    public void BlockIncludePlanetMoons()
    {
        IsIncludePlanetMoonsBlocked = true;
    }

    public void HidePlanetsWithPrivateName()
    {
        ArePlanetsWithPrivateNameHidden = true;
    }

    public void AutoIncludeStarGivingLightToMoon()
    {
        IsStarGivingLightToMoonAutoIncluded = true;
    }
}
