#nullable disable

using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace JsonApiDotNetCore.Controllers
{
    /// <summary>
    /// Represents the violation of a model state validation rule.
    /// </summary>
    [PublicAPI]
    public sealed class ModelStateViolation
    {
        public string Prefix { get; }
        public string PropertyName { get; }
        public Type ResourceClrType { get; set; }
        public ModelError Error { get; }

        public ModelStateViolation(string prefix, string propertyName, Type resourceClrType, ModelError error)
        {
            ArgumentGuard.NotNullNorEmpty(prefix, nameof(prefix));
            ArgumentGuard.NotNullNorEmpty(propertyName, nameof(propertyName));
            ArgumentGuard.NotNull(resourceClrType, nameof(resourceClrType));
            ArgumentGuard.NotNull(error, nameof(error));

            Prefix = prefix;
            PropertyName = propertyName;
            ResourceClrType = resourceClrType;
            Error = error;
        }
    }
}
