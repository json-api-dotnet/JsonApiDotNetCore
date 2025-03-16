using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;

internal abstract class RelationshipsInUpdateRequest;

// ReSharper disable once UnusedTypeParameter
internal sealed class RelationshipsInUpdateRequest<TResource> : RelationshipsInUpdateRequest
    where TResource : IIdentifiable;
