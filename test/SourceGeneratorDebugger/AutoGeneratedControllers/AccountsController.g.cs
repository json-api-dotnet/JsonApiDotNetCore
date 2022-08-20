using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using SourceGeneratorDebugger.Models;

namespace Some.Namespace.To.Place.Controllers;

public sealed partial class AccountsController : JsonApiQueryController<Account, string>
{
    public AccountsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceQueryService<Account, string> queryService)
        : base(options, resourceGraph, loggerFactory, queryService)
    {
    }
}
