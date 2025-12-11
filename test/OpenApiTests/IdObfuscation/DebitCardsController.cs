using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace OpenApiTests.IdObfuscation;

public sealed class DebitCardsController(
    IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory, IResourceService<DebitCard, long> resourceService)
    : ObfuscatedIdentifiableController<DebitCard>(options, resourceGraph, loggerFactory, resourceService);
