using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceConstructorInjection;

/// <summary>
/// A compound type that requires a constructor dependency. Compound types are created without dependency injection, so this cannot be instantiated.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class Stamp(TimeProvider timeProvider)
{
    [Attr]
    public string Country { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; } = timeProvider.GetUtcNow();
}
