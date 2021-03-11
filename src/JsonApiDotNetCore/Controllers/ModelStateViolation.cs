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
        public Type ResourceType { get; set; }
        public ModelError Error { get; }

        public ModelStateViolation(string prefix, string propertyName, Type resourceType, ModelError error)
        {
            ArgumentGuard.NotNullNorEmpty(prefix, nameof(prefix));
            ArgumentGuard.NotNullNorEmpty(propertyName, nameof(propertyName));
            ArgumentGuard.NotNull(resourceType, nameof(resourceType));
            ArgumentGuard.NotNull(error, nameof(error));

            Prefix = prefix;
            PropertyName = propertyName;
            ResourceType = resourceType;
            Error = error;
        }
    }
}
