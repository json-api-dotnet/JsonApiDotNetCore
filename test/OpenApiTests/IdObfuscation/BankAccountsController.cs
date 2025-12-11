using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace OpenApiTests.IdObfuscation;

public sealed class BankAccountsController(
    IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory, IResourceService<BankAccount, long> resourceService)
    : ObfuscatedIdentifiableController<BankAccount>(options, resourceGraph, loggerFactory, resourceService);
