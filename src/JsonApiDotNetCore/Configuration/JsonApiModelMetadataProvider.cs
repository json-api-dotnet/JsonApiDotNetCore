using Microsoft.AspNetCore.Http;
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
        public JsonApiModelMetadataProvider(ICompositeMetadataDetailsProvider detailsProvider, IHttpContextAccessor httpContextAccessor)
            : base(detailsProvider)
        {
            _jsonApiValidationFilter = new JsonApiValidationFilter(httpContextAccessor);
        }

        /// <inheritdoc />
        public JsonApiModelMetadataProvider(ICompositeMetadataDetailsProvider detailsProvider, IOptions<MvcOptions> optionsAccessor,
            IHttpContextAccessor httpContextAccessor)
            : base(detailsProvider, optionsAccessor)
        {
            _jsonApiValidationFilter = new JsonApiValidationFilter(httpContextAccessor);
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
