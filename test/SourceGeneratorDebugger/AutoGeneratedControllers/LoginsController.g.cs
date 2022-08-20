using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using SourceGeneratorDebugger.Models;

namespace SourceGeneratorDebugger.Controllers;

public sealed partial class LoginsController : JsonApiCommandController<Login, int>
{
    public LoginsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceCommandService<Login, int> commandService)
        : base(options, resourceGraph, loggerFactory, commandService)
    {
    }
}
