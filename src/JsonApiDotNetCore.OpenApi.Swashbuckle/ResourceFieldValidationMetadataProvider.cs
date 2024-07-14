using System.ComponentModel.DataAnnotations;
using System.Reflection;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

internal sealed class ResourceFieldValidationMetadataProvider
{
    private readonly IJsonApiOptions _options;
    private readonly IModelMetadataProvider _modelMetadataProvider;

    public ResourceFieldValidationMetadataProvider(IJsonApiOptions options, IModelMetadataProvider modelMetadataProvider)
    {
        ArgumentGuard.NotNull(options);
        ArgumentGuard.NotNull(modelMetadataProvider);

        _options = options;
        _modelMetadataProvider = modelMetadataProvider;
    }

    public bool IsNullable(ResourceFieldAttribute field)
    {
        ArgumentGuard.NotNull(field);

        if (field is HasManyAttribute)
        {
            return false;
        }

        bool hasRequiredAttribute = field.Property.HasAttribute<RequiredAttribute>();

        if (_options.ValidateModelState && hasRequiredAttribute)
        {
            return false;
        }

        NullabilityInfoContext nullabilityContext = new();
        NullabilityInfo nullabilityInfo = nullabilityContext.Create(field.Property);
        return nullabilityInfo.ReadState != NullabilityState.NotNull;
    }

    public bool IsRequired(ResourceFieldAttribute field)
    {
        ArgumentGuard.NotNull(field);

        bool hasRequiredAttribute = field.Property.HasAttribute<RequiredAttribute>();

        if (!_options.ValidateModelState)
        {
            return hasRequiredAttribute;
        }

        if (field is HasManyAttribute)
        {
            return false;
        }

        NullabilityInfoContext nullabilityContext = new();
        NullabilityInfo nullabilityInfo = nullabilityContext.Create(field.Property);
        bool isRequiredValueType = field.Property.PropertyType.IsValueType && hasRequiredAttribute && nullabilityInfo.ReadState == NullabilityState.NotNull;

        if (isRequiredValueType)
        {
            // Special case: ASP.NET ModelState Validation effectively ignores value types with [Required].
            return false;
        }

        return IsModelStateValidationRequired(field);
    }

    private bool IsModelStateValidationRequired(ResourceFieldAttribute field)
    {
        ModelMetadata modelMetadata = _modelMetadataProvider.GetMetadataForProperty(field.Type.ClrType, field.Property.Name);

        // Non-nullable reference types are implicitly required, unless SuppressImplicitRequiredAttributeForNonNullableReferenceTypes is set.
        return modelMetadata.ValidatorMetadata.Any(validatorMetadata => validatorMetadata is RequiredAttribute);
    }
}
