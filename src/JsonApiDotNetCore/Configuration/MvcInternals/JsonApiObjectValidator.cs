// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace JsonApiDotNetCore.Configuration
{
    /// <summary>
    /// The default implementation of <see cref="IObjectModelValidator"/>.
    /// </summary>
    internal class JsonApiObjectValidator : ObjectModelValidator
    {
        private readonly IModelMetadataProvider _modelMetadataProvider;
        private readonly MvcOptions _mvcOptions;
        private readonly ValidatorCache _validatorCache;
        private readonly CompositeModelValidatorProvider _validatorProvider;

        /// <summary>
        /// Initializes a new instance of <see cref="DefaultObjectValidator"/>.
        /// </summary>
        /// <param name="modelMetadataProvider">The <see cref="IModelMetadataProvider"/>.</param>
        /// <param name="validatorProviders">The list of <see cref="IModelValidatorProvider"/>.</param>
        /// <param name="mvcOptions">Accessor to <see cref="MvcOptions"/>.</param>
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

        /// <summary>
        /// Validates the provided object model.
        /// If <paramref name="model"/> is <see langword="null"/> and the <paramref name="metadata"/>'s
        /// <see cref="ModelMetadata.IsRequired"/> is <see langword="true"/>, will add one or more
        /// model state errors that <see cref="Validate(ActionContext, ValidationStateDictionary, string, object)"/>
        /// would not.
        /// </summary>
        /// <param name="actionContext">The <see cref="ActionContext"/>.</param>
        /// <param name="validationState">The <see cref="ValidationStateDictionary"/>.</param>
        /// <param name="prefix">The model prefix key.</param>
        /// <param name="model">The model object.</param>
        /// <param name="metadata">The <see cref="ModelMetadata"/>.</param>
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
