using System.ComponentModel.DataAnnotations;
using System.Reflection;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace JsonApiDotNetCore.Configuration
{
    public sealed class JsonApiModelValidationProvider : IModelValidatorProvider
    {
        private static readonly FieldInfo _validatorMetadataBackingField;
        static JsonApiModelValidationProvider()
        {
           _validatorMetadataBackingField = typeof(ValidatorItem).GetField("<ValidatorMetadata>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);

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
    }
}
