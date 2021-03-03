using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Links
{
    public sealed class PhotosController : JsonApiController<Photo, Guid>
    {
        public PhotosController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Photo, Guid> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
