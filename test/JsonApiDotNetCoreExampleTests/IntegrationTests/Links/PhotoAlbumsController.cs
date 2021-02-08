using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Links
{
    public sealed class PhotoAlbumsController : JsonApiController<PhotoAlbum, Guid>
    {
        public PhotoAlbumsController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<PhotoAlbum, Guid> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
