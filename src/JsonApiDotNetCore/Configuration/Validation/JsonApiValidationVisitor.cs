using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace JsonApiDotNetCore.Configuration.Validation
{
    /// <summary>
    /// An extension of the internal <see cref="ValidationVisitor"/> that performs an additional check related to
    /// property validation filters
    /// </summary>
    /// <remarks>
    /// see  **ADD URL TO ASPNETCORE ISSUE RELATED TO THIS** for background information.
    /// </remarks>
    internal sealed class JsonApiValidationVisitor : ValidationVisitor
    {
        /// <inheritdoc />
        public JsonApiValidationVisitor(
            ActionContext actionContext,
            IModelValidatorProvider validatorProvider,
            ValidatorCache validatorCache,
            IModelMetadataProvider metadataProvider,
            ValidationStateDictionary validationState) 
            : base(actionContext, validatorProvider, validatorCache, metadataProvider, validationState) { }
        
        /// <inheritdoc />
        protected override bool VisitChildren(IValidationStrategy strategy)
        {
            var isValid = true;
            var enumerator = strategy.GetChildren(Metadata, Key, Model);
            var parentEntry = new ValidationEntry(Metadata, Key, Model);

            while (enumerator.MoveNext())
            {
                var entry = enumerator.Current;
                var metadata = entry.Metadata;
                var key = entry.Key;
                
                var jsonApiFilter = metadata.PropertyValidationFilter as PartialPatchValidationFilter;
                var serviceProvider = Context?.HttpContext?.RequestServices;
                
                if (metadata.PropertyValidationFilter?.ShouldValidateEntry(entry, parentEntry) == false 
                    || jsonApiFilter != null && jsonApiFilter.ShouldValidateEntry(entry, parentEntry, serviceProvider) == false  )
                {
                    SuppressValidation(key);
                    continue;
                }

                isValid &= Visit(metadata, key, entry.Model);
            }

            return isValid;
        }
    }
}
