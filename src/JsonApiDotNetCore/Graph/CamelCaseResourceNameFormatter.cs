using System;
using System.Reflection;
using Humanizer;
using JsonApiDotNetCore.Models;
using str = JsonApiDotNetCore.Extensions.StringExtensions;

namespace JsonApiDotNetCore.Graph
{
    public class CamelCaseResourceNameFormatter : IResourceNameFormatter
    {
        /// <summary>
        /// Uses the internal type name to determine the external resource name.
        /// By default we us Humanizer for pluralization and then we dasherize the name.
        /// </summary>
        /// <example>
        /// <code>
        /// _default.FormatResourceName(typeof(TodoItem)).Dump(); 
        /// // > "todo-items"
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
        /// // > "todo-items"
        ///
        /// _default.ApplyCasingConvention("TodoItem"); 
        /// // > "todo-item"
        /// </code>
        /// </example>
        public string ApplyCasingConvention(string properName) => str.Camelize(properName);

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
        /// // > "compound-property"
        /// </code>
        /// </example>
        public string FormatPropertyName(PropertyInfo property) => str.Camelize(property.Name);
    }
}
