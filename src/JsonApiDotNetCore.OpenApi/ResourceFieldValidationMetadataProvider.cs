using System.ComponentModel.DataAnnotations;
using System.Reflection;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi;

internal sealed class ResourceFieldValidationMetadataProvider
{
    private readonly bool _validateModelState;
    private readonly NullabilityInfoContext _nullabilityContext;
    private readonly IModelMetadataProvider _modelMetadataProvider;

    public ResourceFieldValidationMetadataProvider(IJsonApiOptions options, IModelMetadataProvider modelMetadataProvider)
    {
        ArgumentGuard.NotNull(options);
        ArgumentGuard.NotNull(modelMetadataProvider);

        _validateModelState = options.ValidateModelState;
        _modelMetadataProvider = modelMetadataProvider;
        _nullabilityContext = new NullabilityInfoContext();
    }

    public bool IsNullable(ResourceFieldAttribute field)
    {
        ArgumentGuard.NotNull(field);

        if (field is HasManyAttribute)
        {
            return false;
        }

        bool hasRequiredAttribute = field.Property.HasAttribute<RequiredAttribute>();
        NullabilityInfo nullabilityInfo = _nullabilityContext.Create(field.Property);

        if (field is HasManyAttribute)
        {
            return false;
        }

        if (hasRequiredAttribute && _validateModelState && nullabilityInfo.ReadState != NullabilityState.NotNull)
        {
            return false;
        }

        return nullabilityInfo.ReadState != NullabilityState.NotNull;
    }

    public bool IsRequired(ResourceFieldAttribute field)
    {
        ArgumentGuard.NotNull(field);

        bool hasRequiredAttribute = field.Property.HasAttribute<RequiredAttribute>();

        if (!_validateModelState)
        {
            return hasRequiredAttribute;
        }

        if (field is HasManyAttribute)
        {
            return false;
        }

        bool isNotNull = HasNullabilityStateNotNull(field);
        bool isRequiredValueType = field.Property.PropertyType.IsValueType && hasRequiredAttribute && isNotNull;

        if (isRequiredValueType)
        {
            return false;
        }

        return IsModelStateValidationRequired(field);
    }

    private bool IsModelStateValidationRequired(ResourceFieldAttribute field)
    {
        ModelMetadata resourceFieldModelMetadata = _modelMetadataProvider.GetMetadataForProperties(field.Type.ClrType)
            .Single(modelMetadata => modelMetadata.PropertyName! == field.Property.Name);

        return resourceFieldModelMetadata.ValidatorMetadata.Any(validatorMetadata => validatorMetadata is RequiredAttribute);
    }

    private bool HasNullabilityStateNotNull(ResourceFieldAttribute field)
    {
        NullabilityInfo resourceFieldNullabilityInfo = _nullabilityContext.Create(field.Property);
        bool hasNullabilityStateNotNull = resourceFieldNullabilityInfo is { ReadState: NullabilityState.NotNull, WriteState: NullabilityState.NotNull };
        return hasNullabilityStateNotNull;
    }
}
