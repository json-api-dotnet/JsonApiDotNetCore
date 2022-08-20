using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using GettingStarted.Models;

namespace GettingStarted.Controllers;

public sealed partial class BooksController : JsonApiController<Book, int>
{
    public BooksController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<Book, int> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
