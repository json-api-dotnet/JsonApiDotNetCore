using System.Net;
using System.Text.Json;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
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

    private protected override void ValidateExtensionInAttributes(string extensionNamespace, string extensionName, ResourceType resourceType,
        Utf8JsonReader reader)
    {
        if (IsOpenApiDiscriminator(extensionNamespace, extensionName))
        {
            const string jsonPointer = $"attributes/{OpenApiMediaTypeExtension.ExtensionNamespace}:{OpenApiMediaTypeExtension.DiscriminatorPropertyName}";
            ValidateOpenApiDiscriminatorValue(resourceType, jsonPointer, reader);
        }
        else
        {
            base.ValidateExtensionInAttributes(extensionNamespace, extensionName, resourceType, reader);
        }
    }

    private protected override void ValidateExtensionInRelationships(string extensionNamespace, string extensionName, ResourceType resourceType,
        Utf8JsonReader reader)
    {
        if (IsOpenApiDiscriminator(extensionNamespace, extensionName))
        {
            const string jsonPointer = $"relationships/{OpenApiMediaTypeExtension.ExtensionNamespace}:{OpenApiMediaTypeExtension.DiscriminatorPropertyName}";
            ValidateOpenApiDiscriminatorValue(resourceType, jsonPointer, reader);
        }
        else
        {
            base.ValidateExtensionInRelationships(extensionNamespace, extensionName, resourceType, reader);
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

    private static void ValidateOpenApiDiscriminatorValue(ResourceType resourceType, string relativeJsonPointer, Utf8JsonReader reader)
    {
        string? discriminatorValue = reader.GetString();

        if (discriminatorValue != resourceType.PublicName)
        {
            var jsonApiException = new JsonApiException(new ErrorObject(HttpStatusCode.Conflict)
            {
                Title = "Incompatible resource type found.",
                Detail =
                    $"Expected {OpenApiMediaTypeExtension.FullyQualifiedOpenApiDiscriminatorPropertyName} with value '{resourceType.PublicName}' instead of '{discriminatorValue}'.",
                Source = new ErrorSource
                {
                    Pointer = relativeJsonPointer
                }
            });

            CapturedThrow(jsonApiException);
        }
    }
}
