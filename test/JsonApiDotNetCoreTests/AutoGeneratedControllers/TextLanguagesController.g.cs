using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations;

public sealed partial class TextLanguagesController : JsonApiController<TextLanguage, System.Guid>
{
    public TextLanguagesController(IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory,
        IResourceService<TextLanguage, System.Guid> resourceService)
        : base(options, resourceGraph, loggerFactory, resourceService)
    {
    }
}
