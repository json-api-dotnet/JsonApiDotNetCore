using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Issue988
{
    public sealed class DocumentTypesController : JsonApiController<DocumentType, Guid>
    {
        public DocumentTypesController(IJsonApiOptions options, ILoggerFactory loggerFactory, IResourceService<DocumentType, Guid> resourceService)
            : base(options, loggerFactory, resourceService)
        {
        }
    }
}
