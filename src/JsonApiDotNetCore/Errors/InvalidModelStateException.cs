using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Serialization;

namespace JsonApiDotNetCore.Errors
{
    /// <summary>
    /// The error that is thrown when model state validation fails.
    /// </summary>
    public class InvalidModelStateException : Exception, IHasMultipleErrors
    {
        public IReadOnlyCollection<Error> Errors { get; }

        public InvalidModelStateException(ModelStateDictionary modelState, Type resourceType,
            bool includeExceptionStackTraceInErrors, NamingStrategy namingStrategy)
        {
            if (modelState == null) throw new ArgumentNullException(nameof(modelState));
            if (resourceType == null) throw new ArgumentNullException(nameof(resourceType));
            if (namingStrategy == null) throw new ArgumentNullException(nameof(namingStrategy));
            
            Errors = FromModelState(modelState, resourceType, includeExceptionStackTraceInErrors, namingStrategy);
        }

        private static IReadOnlyCollection<Error> FromModelState(ModelStateDictionary modelState, Type resourceType,
            bool includeExceptionStackTraceInErrors, NamingStrategy namingStrategy)
        {
            List<Error> errors = new List<Error>();

            foreach (var (propertyName, entry) in modelState.Where(x => x.Value.Errors.Any()))
            {
                string attributeName = GetDisplayNameForProperty(propertyName, resourceType, namingStrategy);

                foreach (var modelError in entry.Errors)
                {
                    if (modelError.Exception is JsonApiException jsonApiException)
                    {
                        errors.Add(jsonApiException.Error);
                    }
                    else
                    {
                        errors.Add(FromModelError(modelError, attributeName, includeExceptionStackTraceInErrors));
                    }
                }
            }

            return errors;
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

        private static Error FromModelError(ModelError modelError, string attributeName,
            bool includeExceptionStackTraceInErrors)
        {
            var error = new Error(HttpStatusCode.UnprocessableEntity)
            {
                Title = "Input validation failed.",
                Detail = modelError.ErrorMessage,
                Source = attributeName == null
                    ? null
                    : new ErrorSource
                    {
                        Pointer = $"/data/attributes/{attributeName}"
                    }
            };

            if (includeExceptionStackTraceInErrors && modelError.Exception != null)
            {
                error.Meta.IncludeExceptionStackTrace(modelError.Exception);
            }

            return error;
        }
    }
}
