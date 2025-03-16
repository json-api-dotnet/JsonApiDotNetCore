using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;

internal abstract class AttributesInUpdateRequest;

// ReSharper disable once UnusedTypeParameter
internal sealed class AttributesInUpdateRequest<TResource> : AttributesInUpdateRequest
    where TResource : IIdentifiable;
