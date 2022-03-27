using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace BackgroundWorkerService.Interop;

internal sealed class FakeMvcBuilder : IMvcCoreBuilder
{
    public IServiceCollection Services { get; } = new ServiceCollection();
    public ApplicationPartManager PartManager { get; } = new();
}
