using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Options;

namespace JsonApiDotNetCore.Configuration
{
    /// <summary>
    /// Custom implementation of <see cref="IModelMetadataProvider"/> that sets an additional <see cref="IPropertyValidationFilter"/>
    /// to support partial patching.
    /// </summary>
    internal class JsonApiModelMetadataProvider : DefaultModelMetadataProvider
    {
        /// <inheritdoc />
        public JsonApiModelMetadataProvider(ICompositeMetadataDetailsProvider detailsProvider)
            : base(detailsProvider) { }
        
        /// <inheritdoc />
        public JsonApiModelMetadataProvider(ICompositeMetadataDetailsProvider detailsProvider, IOptions<MvcOptions> optionsAccessor) 
            : base(detailsProvider, optionsAccessor) { }
        
        /// <inheritdoc />
        protected override ModelMetadata CreateModelMetadata(DefaultMetadataDetails entry)
        {
            var metadata = new DefaultModelMetadata(this, DetailsProvider, entry, ModelBindingMessageProvider) ;

            var isRequired = metadata.ValidationMetadata.IsRequired;
            
            if (isRequired != null && isRequired.Value)
            {
                metadata.ValidationMetadata.PropertyValidationFilter = new PartialPatchValidationFilter();
            }
            
            return metadata;
        }
    }
}
