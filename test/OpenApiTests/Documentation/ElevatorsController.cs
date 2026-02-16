using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.QueryStrings;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace OpenApiTests.Documentation;

[DisableQueryString(JsonApiQueryStringParameters.Filter | JsonApiQueryStringParameters.Fields)]
public sealed class ElevatorsController(
    IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory, IResourceService<Elevator, long> resourceService)
    : JsonApiController<Elevator, long>(options, resourceGraph, loggerFactory, resourceService);
