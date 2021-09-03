using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations
{
    public sealed class TextLanguagesController : JsonApiController<TextLanguage, Guid>
    {
        public TextLanguagesController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<TextLanguage, Guid> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
