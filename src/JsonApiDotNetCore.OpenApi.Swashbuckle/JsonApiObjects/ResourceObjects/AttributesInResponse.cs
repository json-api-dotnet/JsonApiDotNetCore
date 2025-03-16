using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.JsonApiObjects.ResourceObjects;

internal abstract class AttributesInResponse;

// ReSharper disable once UnusedTypeParameter
internal sealed class AttributesInResponse<TResource> : AttributesInResponse
    where TResource : IIdentifiable;
