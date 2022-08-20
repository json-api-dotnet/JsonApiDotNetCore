using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using ReportsExample.Models;

namespace ReportsExample.Controllers;

public sealed partial class ReportsController : JsonApiController<Report, int>
{
    public ReportsController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IGetAllService<Report, int> getAll)
        : base(options, resourceGraph, loggerFactory,
            getAll: getAll)
    {
    }
}
