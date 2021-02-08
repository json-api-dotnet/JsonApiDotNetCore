using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.QueryStrings
{
    public sealed class CommentsController : JsonApiController<Comment>
    {
        public CommentsController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<Comment> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
