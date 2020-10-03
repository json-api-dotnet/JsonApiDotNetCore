using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace JsonApiDotNetCore.Configuration
{
    /// <summary>
    /// Custom implementation of <see cref="IObjectModelValidator"/> that is identical to DefaultObjectValidator, apart from
    /// using our own <see cref="JsonApiValidationVisitor"/> instead of the built-in <see cref="ValidationVisitor"/>.
    /// </summary>
    /// <remarks>
    /// See https://github.com/dotnet/aspnetcore/blob/v3.1.8/src/Mvc/Mvc.Core/src/ModelBinding/Validation/DefaultObjectValidator.cs
    /// </remarks>
    internal class JsonApiObjectValidator : ObjectModelValidator
    {
        private readonly IModelMetadataProvider _modelMetadataProvider;
        private readonly MvcOptions _mvcOptions;
        private readonly ValidatorCache _validatorCache;
        private readonly CompositeModelValidatorProvider _validatorProvider;
        
        /// <inheritdoc />
        public JsonApiObjectValidator(
            IModelMetadataProvider modelMetadataProvider,
            IList<IModelValidatorProvider> validatorProviders,
            MvcOptions mvcOptions)
            : base(modelMetadataProvider, validatorProviders)
        {
            if (validatorProviders == null)
            {
                throw new ArgumentNullException(nameof(validatorProviders));
            }

            _modelMetadataProvider = modelMetadataProvider ?? throw new ArgumentNullException(nameof(modelMetadataProvider));
            _validatorCache = new ValidatorCache();
            _validatorProvider = new CompositeModelValidatorProvider(validatorProviders);
            _mvcOptions = mvcOptions;
        }

        /// <inheritdoc />
        public override ValidationVisitor GetValidationVisitor(
            ActionContext actionContext,
            IModelValidatorProvider validatorProvider,
            ValidatorCache validatorCache,
            IModelMetadataProvider metadataProvider,
            ValidationStateDictionary validationState)
        {
            var visitor = new JsonApiValidationVisitor(
                actionContext,
                validatorProvider,
                validatorCache,
                metadataProvider,
                validationState)
            {
                MaxValidationDepth = _mvcOptions.MaxValidationDepth,
                ValidateComplexTypesIfChildValidationFails = _mvcOptions.ValidateComplexTypesIfChildValidationFails,
            };

            return visitor;
        }
        
        /// <inheritdoc />
        public override void Validate(
            ActionContext actionContext,
            ValidationStateDictionary validationState,
            string prefix,
            object model)
        {
            var visitor = GetValidationVisitor(
                actionContext,
                _validatorProvider,
                _validatorCache,
                _modelMetadataProvider,
                validationState);

            var metadata = model == null ? null : _modelMetadataProvider.GetMetadataForType(model.GetType());
            visitor.Validate(metadata, prefix, model, alwaysValidateAtTopLevel: false);
        }
        
        /// <inheritdoc />
        public override void Validate(
            ActionContext actionContext,
            ValidationStateDictionary validationState,
            string prefix,
            object model,
            ModelMetadata metadata)
        {
            var visitor = GetValidationVisitor(
                actionContext,
                _validatorProvider,
                _validatorCache,
                _modelMetadataProvider,
                validationState);
            
            visitor.Validate(metadata, prefix, model, alwaysValidateAtTopLevel: metadata.IsRequired);
        }
    }
}
