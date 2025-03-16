using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;

internal abstract class RelationshipsInCreateRequest;

// ReSharper disable once UnusedTypeParameter
internal sealed class RelationshipsInCreateRequest<TResource> : RelationshipsInCreateRequest
    where TResource : IIdentifiable;
