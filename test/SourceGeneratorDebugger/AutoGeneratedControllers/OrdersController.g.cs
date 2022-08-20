using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using SourceGeneratorDebugger.Models;

namespace SourceGeneratorDebugger.Controllers;

public sealed partial class OrdersController : JsonApiController<Order, long>
{
    public OrdersController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<Order, long> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
