using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations;

public sealed partial class RecordCompaniesController : JsonApiController<RecordCompany, short>
{
    public RecordCompaniesController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<RecordCompany, short> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
