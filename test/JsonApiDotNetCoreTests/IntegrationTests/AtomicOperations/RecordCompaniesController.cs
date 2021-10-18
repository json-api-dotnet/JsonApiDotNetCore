#nullable disable

using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations
{
    public sealed class RecordCompaniesController : JsonApiController<RecordCompany, short>
    {
        public RecordCompaniesController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
            IResourceService<RecordCompany, short> resourceService)
            : base(options, resourceGraph, loggerFactory, resourceService)
        {
        }
    }
}
