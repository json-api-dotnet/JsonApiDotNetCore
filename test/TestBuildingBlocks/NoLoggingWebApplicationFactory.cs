using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;

namespace TestBuildingBlocks;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public sealed class NoLoggingWebApplicationFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint>
    where TEntryPoint : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        DisableLogging(builder);
    }

    [Conditional("RELEASE")]
    private static void DisableLogging(IWebHostBuilder builder)
    {
        // Disable logging to keep the output from C/I build clean. Errors are expected to occur while testing failure handling.
        builder.ConfigureLogging(loggingBuilder => loggingBuilder.ClearProviders());
    }
}
