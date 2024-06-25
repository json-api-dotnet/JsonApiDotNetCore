using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.ContentNegotiation;

public sealed class OperationsController(
    IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory, IOperationsProcessor processor, IJsonApiRequest request,
    ITargetedFields targetedFields, IAtomicOperationFilter operationFilter) : JsonApiOperationsController(options, resourceGraph, loggerFactory, processor,
    request, targetedFields, operationFilter);
