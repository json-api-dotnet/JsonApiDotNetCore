using JetBrains.Annotations;

namespace OpenApiTests.LegacyOpenApiIntegration;

/// <summary>
/// Lists the various airlines used in this API.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public enum Airline : byte
{
    DeltaAirLines,
    LufthansaGroup,
    AirFranceKlm
}
