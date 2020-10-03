using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace JsonApiDotNetCore.Configuration
{
    /// <summary>
    /// This model validator provider does not create any validators, but is used to indirectly change the behavior of
    /// the internal DataAnnotationsModelValidatorProvider through the shared <see cref="ModelValidatorProviderContext"/> object.
    /// See https://github.com/json-api-dotnet/JsonApiDotNetCore/pull/847 for more info.
    /// </summary>
    internal sealed class JsonApiModelValidationProvider : IMetadataBasedModelValidatorProvider
    {
        private static readonly FieldInfo _validatorMetadataBackingField;
        
        static JsonApiModelValidationProvider()
        {
           _validatorMetadataBackingField = typeof(ValidatorItem).GetField($"<{nameof(ValidatorItem.ValidatorMetadata)}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
        }   
        
        public void CreateValidators(ModelValidatorProviderContext context)
        {
            foreach (var item in context.Results)
            {
                if (item.ValidatorMetadata.GetType() == typeof(RequiredAttribute))
                {
                    _validatorMetadataBackingField.SetValue(item, new JsonApiRequiredAttribute());
                }
            }
        }

        /// <summary>
        /// Returns false to ensure no further validation is executed through this provider.
        /// </summary>
        public bool HasValidators(Type modelType, IList<object> validatorMetadata) => false;
    }
}
