using System;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Errors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace JsonApiDotNetCore.AtomicOperations.Processors
{
    public abstract class BaseAtomicOperationProcessor
    {
        private readonly IJsonApiOptions _options;
        private readonly IObjectModelValidator _validator;

        protected BaseAtomicOperationProcessor(IJsonApiOptions options, IObjectModelValidator validator)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        protected void ValidateModelState<TResource>(TResource model)
        {
            if (_options.ValidateModelState)
            {
                var actionContext = new ActionContext();
                _validator.Validate(actionContext, null, string.Empty, model);

                if (!actionContext.ModelState.IsValid)
                {
                    var namingStrategy = _options.SerializerContractResolver.NamingStrategy;
                    throw new InvalidModelStateException(actionContext.ModelState, typeof(TResource), _options.IncludeExceptionStackTraceInErrors, namingStrategy);
                }
            }
        }
    }
}
