namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading;

internal sealed class TestClientSettingsProvider : IClientSettingsProvider
{
    public bool AreVeryLargeStarsHidden { get; private set; }
    public bool AreConstellationsVisibleDuringWinterHidden { get; private set; }
    public bool IsIncludePlanetMoonsBlocked { get; private set; }
    public bool ArePlanetsWithPrivateNameHidden { get; private set; }
    public bool IsStarGivingLightToMoonAutoIncluded { get; private set; }

    public void ResetToDefaults()
    {
        AreVeryLargeStarsHidden = false;
        AreConstellationsVisibleDuringWinterHidden = false;
        IsIncludePlanetMoonsBlocked = false;
        ArePlanetsWithPrivateNameHidden = false;
        IsStarGivingLightToMoonAutoIncluded = false;
    }

    public void HideVeryLargeStars()
    {
        AreVeryLargeStarsHidden = true;
    }

    public void HideConstellationsVisibleDuringWinter()
    {
        AreConstellationsVisibleDuringWinterHidden = true;
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
