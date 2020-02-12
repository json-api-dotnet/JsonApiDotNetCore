using System;
using System.Reflection;
using Humanizer;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Graph
{
    public abstract class BaseResourceNameFormatter : IResourceNameFormatter
    {
        /// <summary>
        /// Uses the internal type name to determine the external resource name.
        /// </summary>
        public string FormatResourceName(Type type)
        {
            try
            {
                // check the class definition first
                // [Resource("models"] public class Model : Identifiable { /* ... */ }
                if (type.GetCustomAttribute(typeof(ResourceAttribute)) is ResourceAttribute attribute)
                    return attribute.ResourceName;

                return ApplyCasingConvention(type.Name.Pluralize());
            }
            catch (InvalidOperationException e)
            {
                throw new InvalidOperationException($"Cannot define multiple {nameof(ResourceAttribute)}s on type '{type}'.", e);
            }
        }

        /// <summary>
        /// Applies the desired casing convention to the internal string.
        /// This is generally applied to the type name after pluralization.
        /// </summary>
        public abstract string ApplyCasingConvention(string properName);

        /// <summary>
        /// Uses the internal PropertyInfo to determine the external resource name.
        /// By default the name will be formatted to camelCase.
        /// </summary>
        public string FormatPropertyName(PropertyInfo property) => ApplyCasingConvention(property.Name);
    }
}
