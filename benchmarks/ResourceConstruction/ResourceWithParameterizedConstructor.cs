using JsonApiDotNetCore;
using JsonApiDotNetCore.Resources;
using Microsoft.AspNetCore.Authentication;

namespace Benchmarks.ResourceConstruction;

public sealed class ResourceWithParameterizedConstructor : Identifiable<long>
{
    private readonly ISystemClock _systemClock;

    public string? Value { get; set; }

    public ResourceWithParameterizedConstructor(ISystemClock systemClock)
    {
        ArgumentGuard.NotNull(systemClock, nameof(systemClock));
        _systemClock = systemClock;
    }
}
