using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;

internal abstract class RelationshipsInResponse;

// ReSharper disable once UnusedTypeParameter
internal sealed class RelationshipsInResponse<TResource> : RelationshipsInResponse
    where TResource : IIdentifiable;
