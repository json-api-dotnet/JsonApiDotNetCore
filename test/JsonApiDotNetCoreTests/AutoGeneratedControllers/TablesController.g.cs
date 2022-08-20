using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.RestrictedControllers;

namespace JsonApiDotNetCoreTests.IntegrationTests.RestrictedControllers;

public sealed partial class TablesController : JsonApiCommandController<Table, int>
{
    public TablesController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceCommandService<Table, int> commandService)
        : base(options, resourceGraph, loggerFactory, commandService)
    {
    }
}
