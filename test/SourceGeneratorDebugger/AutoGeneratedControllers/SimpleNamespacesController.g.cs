using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using SourceGeneratorDebugger;

namespace Controllers;

public sealed partial class SimpleNamespacesController : JsonApiController<SimpleNamespace, int>
{
    public SimpleNamespacesController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<SimpleNamespace, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
