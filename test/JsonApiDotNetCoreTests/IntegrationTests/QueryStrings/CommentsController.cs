#nullable disable

using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings
{
    public sealed class CommentsController : JsonApiController<Comment, int>
    {
        public CommentsController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<Comment, int> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
