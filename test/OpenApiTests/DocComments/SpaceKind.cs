using JetBrains.Annotations;

namespace OpenApiTests.DocComments;

/// <summary>
/// Lists the various kinds of spaces within a skyscraper.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public enum SpaceKind
{
    Office,
    Hotel,
    Residential,
    Retail
}
