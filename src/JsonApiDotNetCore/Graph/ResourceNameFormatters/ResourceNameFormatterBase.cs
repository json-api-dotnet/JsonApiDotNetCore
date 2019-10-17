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
        /// By default we us Humanizer for pluralization and then we dasherize the name.
        /// </summary>
        /// <example>
        /// <code>
        /// _default.FormatResourceName(typeof(TodoItem)).Dump(); 
        /// // > "todoItems"
        /// </code>
        /// </example>
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
        /// Aoplies the desired casing convention to the internal string.
        /// This is generally applied to the type name after pluralization.
        /// </summary>
        ///
        /// <example>
        /// <code>
        /// _default.ApplyCasingConvention("TodoItems"); 
        /// // > "todoItems"
        ///
        /// _default.ApplyCasingConvention("TodoItem"); 
        /// // > "todoItem"
        /// </code>
        /// </example>
        public abstract string ApplyCasingConvention(string properName);

        /// <summary>
        /// Uses the internal PropertyInfo to determine the external resource name.
        /// By default the name will be formatted to kebab-case.
        /// </summary>
        /// <example>
        /// Given the following property:
        /// <code>
        /// public string CompoundProperty { get; set; }
        /// </code>
        /// The public attribute will be formatted like so:
        /// <code>
        /// _default.FormatPropertyName(compoundProperty).Dump(); 
        /// // > "compoundProperty"
        /// </code>
        /// </example>
        public string FormatPropertyName(PropertyInfo property) => ApplyCasingConvention(property.Name);
    }
}
