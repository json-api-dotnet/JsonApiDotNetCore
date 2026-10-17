namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading;

internal sealed class TestClientSettingsProvider : IClientSettingsProvider
{
    public bool AreVeryLargeStarsHidden { get; private set; }
    public bool AreConstellationsVisibleDuringWinterHidden { get; private set; }
    public bool IsIncludePlanetMoonsBlocked { get; private set; }
    public bool ArePlanetsWithPrivateNameHidden { get; private set; }
    public bool IsStarGivingLightToMoonAutoIncluded { get; private set; }
    public PlanetFieldSetBehavior PlanetFieldSetBehavior { get; private set; }
    public bool IsPlanetPageSizeLimited { get; private set; }
    public bool AreSmallMoonsHidden { get; private set; }
    public bool IsConstellationStarsIncludeRewritten { get; private set; }

    public void ResetToDefaults()
    {
        PlanetFieldSetBehavior = PlanetFieldSetBehavior.Unchanged;
        IsPlanetPageSizeLimited = false;
        AreSmallMoonsHidden = false;
        IsConstellationStarsIncludeRewritten = false;
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

    public void SetPlanetFieldSetBehavior(PlanetFieldSetBehavior behavior)
    {
        PlanetFieldSetBehavior = behavior;
    }

    public void LimitPlanetPageSize()
    {
        IsPlanetPageSizeLimited = true;
    }

    public void HideSmallMoons()
    {
        AreSmallMoonsHidden = true;
    }

    public void RewriteConstellationStarsInclude()
    {
        IsConstellationStarsIncludeRewritten = true;
    }
}
