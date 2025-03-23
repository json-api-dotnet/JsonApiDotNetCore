using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;

internal abstract class AttributesInCreateRequest;

// ReSharper disable once UnusedTypeParameter
internal sealed class AttributesInCreateRequest<TResource> : AttributesInCreateRequest
    where TResource : IIdentifiable;
