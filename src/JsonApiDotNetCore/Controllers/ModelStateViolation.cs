using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace JsonApiDotNetCore.Controllers
{
    /// <summary>
    /// Represents the violation of a model state validation rule.
    /// </summary>
    public sealed class ModelStateViolation
    {
        public string Prefix { get; }
        public string PropertyName { get; }
        public Type ResourceType { get; set; }
        public ModelError Error { get; }

        public ModelStateViolation(string prefix, string propertyName, Type resourceType, ModelError error)
        {
            Prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
            PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
            ResourceType = resourceType ?? throw new ArgumentNullException(nameof(resourceType));
            Error = error ?? throw new ArgumentNullException(nameof(error));
        }
    }
}
