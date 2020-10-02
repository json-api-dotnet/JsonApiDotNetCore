// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace JsonApiDotNetCore.Configuration
{
    /// <summary>
    /// An implementation of <see cref="IModelValidatorProvider"/> which provides validators
    /// for attributes which derive from <see cref="ValidationAttribute"/>. It also provides
    /// a validator for types which implement <see cref="IValidatableObject"/>.
    /// </summary>
    internal sealed class CustomProvider : IMetadataBasedModelValidatorProvider
    {
        private readonly IOptions<MvcDataAnnotationsLocalizationOptions> _options;
        private readonly IStringLocalizerFactory _stringLocalizerFactory;
        private readonly IValidationAttributeAdapterProvider _validationAttributeAdapterProvider;

        /// <summary>
        /// Create a new instance of <see cref="DataAnnotationsModelValidatorProvider"/>.
        /// </summary>
        /// <param name="validationAttributeAdapterProvider">The <see cref="IValidationAttributeAdapterProvider"/>
        /// that supplies <see cref="IAttributeAdapter"/>s.</param>
        /// <param name="options">The <see cref="IOptions{MvcDataAnnotationsLocalizationOptions}"/>.</param>
        /// <param name="stringLocalizerFactory">The <see cref="IStringLocalizerFactory"/>.</param>
        /// <remarks><paramref name="options"/> and <paramref name="stringLocalizerFactory"/>
        /// are nullable only for testing ease.</remarks>
        public CustomProvider(
            IValidationAttributeAdapterProvider validationAttributeAdapterProvider,
            IOptions<MvcDataAnnotationsLocalizationOptions> options,
            IStringLocalizerFactory stringLocalizerFactory)
        {
            if (validationAttributeAdapterProvider == null)
            {
                throw new ArgumentNullException(nameof(validationAttributeAdapterProvider));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _validationAttributeAdapterProvider = validationAttributeAdapterProvider;
            _options = options;
            _stringLocalizerFactory = stringLocalizerFactory;
        }

        public void CreateValidators(ModelValidatorProviderContext context)
        {
            IStringLocalizer stringLocalizer = null;
            if (_stringLocalizerFactory != null && _options.Value.DataAnnotationLocalizerProvider != null)
            {
                stringLocalizer = _options.Value.DataAnnotationLocalizerProvider(
                    context.ModelMetadata.ContainerType ?? context.ModelMetadata.ModelType,
                    _stringLocalizerFactory);
            }

            var results = context.Results;
            // Read interface .Count once rather than per iteration
            var resultsCount = results.Count;
            for (var i = 0; i < resultsCount; i++)
            {
                var validatorItem = results[i];
                if (validatorItem.Validator != null)
                {
                    continue;
                }

                if (!(validatorItem.ValidatorMetadata is ValidationAttribute attribute))
                {
                    continue;
                }
                
                var validator = new CustomValidator(
                    _validationAttributeAdapterProvider,
                    attribute,
                    stringLocalizer);

                validatorItem.Validator = validator;
                validatorItem.IsReusable = true;
                // Inserts validators based on whether or not they are 'required'. We want to run
                // 'required' validators first so that we get the best possible error message.
                if (attribute is RequiredAttribute)
                {
                    context.Results.Remove(validatorItem);
                    context.Results.Insert(0, validatorItem);
                }
            }

            // Produce a validator if the type supports IValidatableObject
            if (typeof(IValidatableObject).IsAssignableFrom(context.ModelMetadata.ModelType))
            {
                context.Results.Add(new ValidatorItem
                {
                    Validator = new ValidatableObjectAdapter(),
                    IsReusable = true
                });
            }
        }

        public bool HasValidators(Type modelType, IList<object> validatorMetadata)
        {
            if (typeof(IValidatableObject).IsAssignableFrom(modelType))
            {
                return true;
            }

            // Read interface .Count once rather than per iteration
            var validatorMetadataCount = validatorMetadata.Count;
            for (var i = 0; i < validatorMetadataCount; i++)
            {
                if (validatorMetadata[i] is ValidationAttribute)
                {
                    return true;
                }
            }

            return false;
        }
    }
    
    
    public class ValidatableObjectAdapter : IModelValidator
    {
        public IEnumerable<ModelValidationResult> Validate(ModelValidationContext context)
        {
            var model = context.Model;
            if (model == null)
            {
                return Enumerable.Empty<ModelValidationResult>();
            }

            if (!(model is IValidatableObject validatable))
            {
                var message = "lol1";

                throw new InvalidOperationException(message);
            }

            // The constructed ValidationContext is intentionally slightly different from what
            // DataAnnotationsModelValidator creates. The instance parameter would be context.Container
            // (if non-null) in that class. But, DataAnnotationsModelValidator _also_ passes context.Model
            // separately to any ValidationAttribute.
            var validationContext = new ValidationContext(
                instance: validatable,
                serviceProvider: context.ActionContext?.HttpContext?.RequestServices,
                items: null)
            {
                DisplayName = context.ModelMetadata.GetDisplayName(),
                MemberName = context.ModelMetadata.Name,
            };

            return ConvertResults(validatable.Validate(validationContext));
        }

        private IEnumerable<ModelValidationResult> ConvertResults(IEnumerable<ValidationResult> results)
        {
            foreach (var result in results)
            {
                if (result != ValidationResult.Success)
                {
                    if (result.MemberNames == null || !result.MemberNames.Any())
                    {
                        yield return new ModelValidationResult(memberName: null, message: result.ErrorMessage);
                    }
                    else
                    {
                        foreach (var memberName in result.MemberNames)
                        {
                            yield return new ModelValidationResult(memberName, result.ErrorMessage);
                        }
                    }
                }
            }
        }
    }
}
