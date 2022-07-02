using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace NoDbConsoleQueryExample.Interop;

/// <summary>
/// Discards any MVC-specific service registrations.
/// </summary>
internal sealed class FakeMvcBuilder : IMvcCoreBuilder
{
    public IServiceCollection Services { get; } = new ServiceCollection();
    public ApplicationPartManager PartManager { get; } = new();
}
