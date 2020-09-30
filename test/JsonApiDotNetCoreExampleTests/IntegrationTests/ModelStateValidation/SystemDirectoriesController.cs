using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ModelStateValidation
{
    public sealed class SystemDirectoriesController : JsonApiController<SystemDirectory>
    {
        public SystemDirectoriesController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<SystemDirectory> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
