using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.AtomicOperations
{
    public sealed class RecordCompaniesController : JsonApiController<RecordCompany, short>
    {
        public RecordCompaniesController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<RecordCompany, short> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
