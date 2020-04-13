using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.JsonApiDocuments;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace JsonApiDotNetCore.Exceptions
{
    /// <summary>
    /// The error that is thrown when model state validation fails.
    /// </summary>
    public class InvalidModelStateException : Exception
    {
        public IList<Error> Errors { get; }

        public InvalidModelStateException(ModelStateDictionary modelState, Type resourceType,
            bool includeExceptionStackTraceInErrors)
        {
            Errors = FromModelState(modelState, resourceType, includeExceptionStackTraceInErrors);
        }

        private static List<Error> FromModelState(ModelStateDictionary modelState, Type resourceType,
            bool includeExceptionStackTraceInErrors)
        {
            List<Error> errors = new List<Error>();

            foreach (var pair in modelState.Where(x => x.Value.Errors.Any()))
            {
                var propertyName = pair.Key;
                PropertyInfo property = resourceType.GetProperty(propertyName);

                // TODO: Need access to ResourceContext here, in order to determine attribute name when not explicitly set.
                string attributeName =
                    property?.GetCustomAttribute<AttrAttribute>().PublicAttributeName ?? property?.Name;

                foreach (var modelError in pair.Value.Errors)
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
