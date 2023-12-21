using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.IdObfuscation;

public sealed class DebitCardsController(
    IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory, IResourceService<DebitCard, int> resourceService)
    : ObfuscatedIdentifiableController<DebitCard>(options, resourceGraph, loggerFactory, resourceService);
