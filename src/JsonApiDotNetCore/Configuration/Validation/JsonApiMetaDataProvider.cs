using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Options;

namespace JsonApiDotNetCore.Configuration.Validation
{
    /// <summary>
    /// A default implementation of <see cref="IModelMetadataProvider"/> based on reflection.
    /// </summary>
    internal class JsonApiModelMetadataProvider : DefaultModelMetadataProvider
    {
        public JsonApiModelMetadataProvider(ICompositeMetadataDetailsProvider detailsProvider)
            : base(detailsProvider) { }

        public JsonApiModelMetadataProvider(ICompositeMetadataDetailsProvider detailsProvider, IOptions<MvcOptions> optionsAccessor) 
            : base(detailsProvider, optionsAccessor) { }
        
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
