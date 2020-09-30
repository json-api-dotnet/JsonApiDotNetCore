
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
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
            var itemsToRemove = new List<ValidatorItem>();
            
            foreach (var item in context.Results)
            {
                if (item.ValidatorMetadata.GetType() == typeof(RequiredAttribute))
                {
                    // var interceptor = new MetadataDetailsProviderListInterceptor(options.ModelMetadataDetailsProviders);
                    var property = item.GetType().GetField("<ValidatorMetadata>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
                    property.SetValue(item, new JsonApiRequiredAttribute());
                    // itemsToRemove.Add(item);
                }
            }

            // foreach (var item in itemsToRemove)
            // {
            //     context.Results.Remove(item);
            // }
        }

        public bool HasValidators(Type modelType, IList<object> validatorMetadata)
        {
            var hasValidators = _internalModelValidatorProvider.HasValidators(modelType, validatorMetadata);
            return hasValidators;
        }


        private void FuckAround(ValidationContext validationContext)
        {
            // var metadata = validationContext.ModelMetadata;
            // var memberName = metadata.Name;
            // var container = validationContext.Container;
            //
            // var context = new ValidationContext(
            //     instance: container ?? validationContext.Model ?? _emptyValidationContextInstance,
            //     serviceProvider: validationContext.ActionContext?.HttpContext?.RequestServices,
            //     items: null)
            // {
            //     DisplayName = metadata.GetDisplayName(),
            //     MemberName = memberName
            // };
        }
    }
    
    public interface IInternalDataAnnotationsMetadataProvider :         IBindingMetadataProvider,
        IDisplayMetadataProvider,
        IValidationMetadataProvider { }
    
    public sealed class JsonApiMetadataProvider :
        IBindingMetadataProvider,
        IDisplayMetadataProvider,
        IValidationMetadataProvider
    {
        private readonly IMetadataDetailsProvider _internalProvider;

        public JsonApiMetadataProvider(IMetadataDetailsProvider internalProvider)
        {
            _internalProvider = internalProvider ?? throw new ArgumentNullException(nameof(internalProvider));
        }

        /// <inheritdoc />
        public void CreateBindingMetadata(BindingMetadataProviderContext context)
        {
            ((IBindingMetadataProvider)_internalProvider).CreateBindingMetadata(context);
        }

        /// <inheritdoc />
        public void CreateDisplayMetadata(DisplayMetadataProviderContext context)
        {
            ((IDisplayMetadataProvider)_internalProvider).CreateDisplayMetadata(context);
        }

        /// <inheritdoc />
        public void CreateValidationMetadata(ValidationMetadataProviderContext context)
        {
            ((IValidationMetadataProvider)_internalProvider).CreateValidationMetadata(context);
            
            var validatorMedata = context.ValidationMetadata.ValidatorMetadata;
            var itemsToReplaceIndices = new List<int>(); 
            
            for (int i = 0; i < validatorMedata.Count; i++)
            {
                if (validatorMedata[i] is RequiredAttribute)
                {
                    itemsToReplaceIndices.Add(i);
                }
            }

            foreach (var index in itemsToReplaceIndices)
            {
                validatorMedata[index] = new JsonApiRequiredAttribute();
            }
        }
    }
}
