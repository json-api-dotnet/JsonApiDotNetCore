using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Options;

namespace JsonApiDotNetCore.Configuration
{
    /// <summary>
    /// Custom implementation of <see cref="IModelMetadataProvider" /> to support JSON:API partial patching.
    /// </summary>
    internal sealed class JsonApiModelMetadataProvider : DefaultModelMetadataProvider
    {
        private readonly JsonApiValidationFilter _jsonApiValidationFilter;

        /// <inheritdoc />
        public JsonApiModelMetadataProvider(ICompositeMetadataDetailsProvider detailsProvider, IRequestScopedServiceProvider serviceProvider)
            : base(detailsProvider)
        {
            _jsonApiValidationFilter = new JsonApiValidationFilter(serviceProvider);
        }

        /// <inheritdoc />
        public JsonApiModelMetadataProvider(ICompositeMetadataDetailsProvider detailsProvider, IOptions<MvcOptions> optionsAccessor,
            IRequestScopedServiceProvider serviceProvider)
            : base(detailsProvider, optionsAccessor)
        {
            _jsonApiValidationFilter = new JsonApiValidationFilter(serviceProvider);
        }

        /// <inheritdoc />
        protected override ModelMetadata CreateModelMetadata(DefaultMetadataDetails entry)
        {
            var metadata = (DefaultModelMetadata)base.CreateModelMetadata(entry);

            if (metadata.ValidationMetadata.IsRequired == true)
            {
                metadata.ValidationMetadata.PropertyValidationFilter = _jsonApiValidationFilter;
            }

            return metadata;
        }
    }
}
