using System.Text.Json;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.JsonConverters;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

internal sealed class OpenApiResourceObjectConverter : ResourceObjectConverter
{
    private readonly IJsonApiRequestAccessor _requestAccessor;

    private bool HasOpenApiExtension
    {
        get
        {
            if (_requestAccessor.Current == null)
            {
                return false;
            }

            return _requestAccessor.Current.Extensions.Contains(OpenApiMediaTypeExtension.OpenApi) ||
                _requestAccessor.Current.Extensions.Contains(OpenApiMediaTypeExtension.RelaxedOpenApi);
        }
    }

    public OpenApiResourceObjectConverter(IResourceGraph resourceGraph, IJsonApiRequestAccessor requestAccessor)
        : base(resourceGraph)
    {
        ArgumentNullException.ThrowIfNull(requestAccessor);

        _requestAccessor = requestAccessor;
    }

    private protected override void ValidateExtensionInAttributes(string extensionNamespace, string extensionName, Utf8JsonReader reader)
    {
        if (!IsOpenApiDiscriminator(extensionNamespace, extensionName))
        {
            base.ValidateExtensionInAttributes(extensionNamespace, extensionName, reader);
        }
    }

    private protected override void ValidateExtensionInRelationships(string extensionNamespace, string extensionName, Utf8JsonReader reader)
    {
        if (!IsOpenApiDiscriminator(extensionNamespace, extensionName))
        {
            base.ValidateExtensionInRelationships(extensionNamespace, extensionName, reader);
        }
    }

    private protected override void WriteExtensionInAttributes(Utf8JsonWriter writer, ResourceObject value)
    {
        if (HasOpenApiExtension)
        {
            writer.WriteString(OpenApiMediaTypeExtension.FullyQualifiedOpenApiDiscriminatorPropertyName, value.Type);
        }
    }

    private protected override void WriteExtensionInRelationships(Utf8JsonWriter writer, ResourceObject value)
    {
        if (HasOpenApiExtension)
        {
            writer.WriteString(OpenApiMediaTypeExtension.FullyQualifiedOpenApiDiscriminatorPropertyName, value.Type);
        }
    }

    private bool IsOpenApiDiscriminator(string extensionNamespace, string extensionName)
    {
        return HasOpenApiExtension && extensionNamespace == OpenApiMediaTypeExtension.ExtensionNamespace &&
            extensionName == OpenApiMediaTypeExtension.DiscriminatorPropertyName;
    }
}
