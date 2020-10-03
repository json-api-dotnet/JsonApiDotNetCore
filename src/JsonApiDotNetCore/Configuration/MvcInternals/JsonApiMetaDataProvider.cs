// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Options;

namespace JsonApiDotNetCore.Configuration
{
    /// <summary>
    /// A default implementation of <see cref="IModelMetadataProvider"/> based on reflection.
    /// </summary>
    public class JsonApiModelMetadataProvider : DefaultModelMetadataProvider
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
