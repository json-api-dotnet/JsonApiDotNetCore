
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace JsonApiDotNetCore.Configuration
{
    public sealed class JsonApiModelValidationProvider : IMetadataBasedModelValidatorProvider
    {
        private readonly IMetadataBasedModelValidatorProvider _internalModelValidatorProvider;
        
        public JsonApiModelValidationProvider(IModelValidatorProvider internalModelValidatorProvider)
        {
            _internalModelValidatorProvider = internalModelValidatorProvider as IMetadataBasedModelValidatorProvider ?? throw new ArgumentNullException(nameof(internalModelValidatorProvider));
        }

        public void CreateValidators(ModelValidatorProviderContext context)
        {
            _internalModelValidatorProvider.CreateValidators(context);
        }

        public bool HasValidators(Type modelType, IList<object> validatorMetadata)
        {
            return _internalModelValidatorProvider.HasValidators(modelType, validatorMetadata);
        }
    }
}
