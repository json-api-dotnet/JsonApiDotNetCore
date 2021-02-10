using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Serialization;

namespace JsonApiDotNetCore.Errors
{
    /// <summary>
    /// The error that is thrown when model state validation fails.
    /// </summary>
    public class InvalidModelStateException : JsonApiException
    {
        public InvalidModelStateException(ModelStateDictionary modelState, Type resourceType,
            bool includeExceptionStackTraceInErrors, NamingStrategy namingStrategy)
            : this(FromModelStateDictionary(modelState, resourceType), includeExceptionStackTraceInErrors, namingStrategy)
        {
        }

        private static IEnumerable<ModelStateViolation> FromModelStateDictionary(ModelStateDictionary modelState, Type resourceType)
        {
            foreach (var (propertyName, entry) in modelState)
            {
                foreach (ModelError error in entry.Errors)
                {
                    yield return new ModelStateViolation("/data/attributes/", propertyName, resourceType, error);
                }
            }
        }

        public InvalidModelStateException(IEnumerable<ModelStateViolation> violations,
            bool includeExceptionStackTraceInErrors, NamingStrategy namingStrategy)
            : base(FromModelStateViolations(violations, includeExceptionStackTraceInErrors, namingStrategy))
        {
        }

        private static IEnumerable<Error> FromModelStateViolations(IEnumerable<ModelStateViolation> violations, 
            bool includeExceptionStackTraceInErrors, NamingStrategy namingStrategy)
        {
            if (violations == null) throw new ArgumentNullException(nameof(violations));
            if (namingStrategy == null) throw new ArgumentNullException(nameof(namingStrategy));

            foreach (var violation in violations)
            {
                if (violation.Error.Exception is JsonApiException jsonApiException)
                {
                    foreach (var error in jsonApiException.Errors)
                    {
                        yield return error;
                    }
                }
                else
                {
                    string attributeName = GetDisplayNameForProperty(violation.PropertyName, violation.ResourceType, namingStrategy);
                    var attributePath = violation.Prefix + attributeName;

                    yield return FromModelError(violation.Error, attributePath, includeExceptionStackTraceInErrors);
                }
            }
        }

        private static string GetDisplayNameForProperty(string propertyName, Type resourceType,
            NamingStrategy namingStrategy)
        {
            PropertyInfo property = resourceType.GetProperty(propertyName);
            if (property != null)
            {
                var attrAttribute = property.GetCustomAttribute<AttrAttribute>();
                return attrAttribute?.PublicName ?? namingStrategy.GetPropertyName(property.Name, false);
            }

            return propertyName;
        }

        private static Error FromModelError(ModelError modelError, string attributePath,
            bool includeExceptionStackTraceInErrors)
        {
            var error = new Error(HttpStatusCode.UnprocessableEntity)
            {
                Title = "Input validation failed.",
                Detail = modelError.ErrorMessage,
                Source = attributePath == null
                    ? null
                    : new ErrorSource
                    {
                        Pointer = attributePath
                    }
            };

            if (includeExceptionStackTraceInErrors && modelError.Exception != null)
            {
                error.Meta.IncludeExceptionStackTrace(modelError.Exception.Demystify());
            }

            return error;
        }
    }
}
