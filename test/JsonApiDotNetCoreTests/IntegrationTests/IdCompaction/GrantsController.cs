using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.IdCompaction;

public sealed class GrantsController(
    IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory, IResourceService<Grant, CompactGuid> resourceService)
    : CompactIdentifiableController<Grant>(options, resourceGraph, loggerFactory, resourceService);
