using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance.Models;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ResourceInheritance.Controllers
{
    public sealed class BooksController : JsonApiController<Book>
    {
        public BooksController(IJsonApiOptions options, ILoggerFactory loggerFactory,
            IResourceService<Book> resourceService)
            : base(options, loggerFactory, resourceService) { }
    }
}
