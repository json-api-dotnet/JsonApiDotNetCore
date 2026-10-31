namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading;

public enum PlanetFieldSetBehavior
{
    Unchanged,
    IncludeAtmospherePressure,
    ExcludeAtmosphereComposition,
    ReturnNull
}
