using JsonApiDotNetCore.AtomicOperations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Controllers.Annotations;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCoreTests.IntegrationTests.AtomicOperations.Controllers;

[DisableRoutingConvention]
[Route("/operations/musicTracks/create")]
public sealed class CreateMusicTrackOperationsController(
    IJsonApiOptions options, IResourceGraph resourceGraph, ILoggerFactory loggerFactory, IOperationsProcessor processor, IJsonApiRequest request,
    ITargetedFields targetedFields)
    : JsonApiOperationsController(options, resourceGraph, loggerFactory, processor, request, targetedFields, OnlyCreateMusicTracksOperationFilter.Instance)
{
    private sealed class OnlyCreateMusicTracksOperationFilter : IAtomicOperationFilter
    {
        public static readonly OnlyCreateMusicTracksOperationFilter Instance = new();

        private OnlyCreateMusicTracksOperationFilter()
        {
        }

        public bool IsEnabled(ResourceType resourceType, WriteOperationKind writeOperation)
        {
            return writeOperation == WriteOperationKind.CreateResource && resourceType.ClrType == typeof(MusicTrack);
        }
    }
}
