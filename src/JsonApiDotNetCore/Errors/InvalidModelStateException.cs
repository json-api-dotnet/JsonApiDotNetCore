#nullable disable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.Json;
using JetBrains.Annotations;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace JsonApiDotNetCore.Errors
{
    /// <summary>
    /// The error that is thrown when model state validation fails.
    /// </summary>
    [PublicAPI]
    public sealed class InvalidModelStateException : JsonApiException
    {
        public InvalidModelStateException(ModelStateDictionary modelState, Type resourceClrType, bool includeExceptionStackTraceInErrors,
            JsonNamingPolicy namingPolicy)
            : this(FromModelStateDictionary(modelState, resourceClrType), includeExceptionStackTraceInErrors, namingPolicy)
        {
        }

        public InvalidModelStateException(IEnumerable<ModelStateViolation> violations, bool includeExceptionStackTraceInErrors, JsonNamingPolicy namingPolicy)
            : base(FromModelStateViolations(violations, includeExceptionStackTraceInErrors, namingPolicy))
        {
        }

        private static IEnumerable<ModelStateViolation> FromModelStateDictionary(ModelStateDictionary modelState, Type resourceClrType)
        {
            ArgumentGuard.NotNull(modelState, nameof(modelState));
            ArgumentGuard.NotNull(resourceClrType, nameof(resourceClrType));

            var violations = new List<ModelStateViolation>();

            foreach ((string propertyName, ModelStateEntry entry) in modelState)
            {
                AddValidationErrors(entry, propertyName, resourceClrType, violations);
            }

            return violations;
        }

        private static void AddValidationErrors(ModelStateEntry entry, string propertyName, Type resourceClrType, List<ModelStateViolation> violations)
        {
            foreach (ModelError error in entry.Errors)
            {
                var violation = new ModelStateViolation("/data/attributes/", propertyName, resourceClrType, error);
                violations.Add(violation);
            }
        }

        private static IEnumerable<ErrorObject> FromModelStateViolations(IEnumerable<ModelStateViolation> violations, bool includeExceptionStackTraceInErrors,
            JsonNamingPolicy namingPolicy)
        {
            ArgumentGuard.NotNull(violations, nameof(violations));

            return violations.SelectMany(violation => FromModelStateViolation(violation, includeExceptionStackTraceInErrors, namingPolicy));
        }

        private static IEnumerable<ErrorObject> FromModelStateViolation(ModelStateViolation violation, bool includeExceptionStackTraceInErrors,
            JsonNamingPolicy namingPolicy)
        {
            if (violation.Error.Exception is JsonApiException jsonApiException)
            {
                foreach (ErrorObject error in jsonApiException.Errors)
                {
                    yield return error;
                }
            }
            else
            {
                string attributeName = GetDisplayNameForProperty(violation.PropertyName, violation.ResourceClrType, namingPolicy);
                string attributePath = $"{violation.Prefix}{attributeName}";

                yield return FromModelError(violation.Error, attributePath, includeExceptionStackTraceInErrors);
            }
        }

        private static string GetDisplayNameForProperty(string propertyName, Type resourceClrType, JsonNamingPolicy namingPolicy)
        {
            PropertyInfo property = resourceClrType.GetProperty(propertyName);

            if (property != null)
            {
                var attrAttribute = property.GetCustomAttribute<AttrAttribute>();

                if (attrAttribute?.PublicName != null)
                {
                    return attrAttribute.PublicName;
                }

                return namingPolicy != null ? namingPolicy.ConvertName(property.Name) : property.Name;
            }

            return propertyName;
        }

        private static ErrorObject FromModelError(ModelError modelError, string attributePath, bool includeExceptionStackTraceInErrors)
        {
            var error = new ErrorObject(HttpStatusCode.UnprocessableEntity)
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
                Exception exception = modelError.Exception.Demystify();
                string[] stackTraceLines = exception.ToString().Split(Environment.NewLine);

                if (stackTraceLines.Any())
                {
                    error.Meta ??= new Dictionary<string, object>();
                    error.Meta["StackTrace"] = stackTraceLines;
                }
            }

            return error;
        }
    }
}
