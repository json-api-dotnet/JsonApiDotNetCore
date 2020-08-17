using System;
using JsonApiDotNetCore.Internal;

namespace JsonApiDotNetCore.Models.Annotation
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class AttrAttribute : ResourceFieldAttribute
    {
        internal bool HasExplicitCapabilities { get; }

        public AttrCapabilities Capabilities { get; internal set; }

        /// <summary>
        /// Exposes a resource property as a json:api attribute using the configured casing convention and capabilities.
        /// </summary>
        /// <example>
        /// <code>
        /// public class Author : Identifiable
        /// {
        ///     [Attr]
        ///     public string Name { get; set; }
        /// }
        /// </code>
        /// </example>
        public AttrAttribute()
        {
        }

        /// <summary>
        /// Exposes a resource property as a json:api attribute with an explicit name, using configured capabilities.
        /// </summary>
        public AttrAttribute(string publicName) 
            : base(publicName)
        {
            if (publicName == null)
            {
                throw new ArgumentNullException(nameof(publicName));
            }
        }

        /// <summary>
        /// Exposes a resource property as a json:api attribute using the configured casing convention and an explicit set of capabilities.
        /// </summary>
        /// <example>
        /// <code>
        /// public class Author : Identifiable
        /// {
        ///     [Attr(AttrCapabilities.AllowFilter | AttrCapabilities.AllowSort)]
        ///     public string Name { get; set; }
        /// }
        /// </code>
        /// </example>
        public AttrAttribute(AttrCapabilities capabilities)
        {
            HasExplicitCapabilities = true;
            Capabilities = capabilities;
        }

        /// <summary>
        /// Exposes a resource property as a json:api attribute with an explicit name and capabilities.
        /// </summary>
        public AttrAttribute(string publicName, AttrCapabilities capabilities) 
            : this(publicName)
        {
            HasExplicitCapabilities = true;
            Capabilities = capabilities;
        }

        /// <summary>
        /// Get the value of the attribute for the given object.
        /// Returns null if the attribute does not belong to the
        /// provided object.
        /// </summary>
        public object GetValue(object resource)
        {
            if (resource == null)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            if (Property.GetMethod == null)
            {
                throw new InvalidOperationException($"Property '{Property.DeclaringType?.Name}.{Property.Name}' is write-only.");
            }

            return Property.GetValue(resource);
        }

        /// <summary>
        /// Sets the value of the attribute on the given object.
        /// </summary>
        public void SetValue(object resource, object newValue)
        {
            if (resource == null)
            {
                throw new ArgumentNullException(nameof(resource));
            }

            if (Property.SetMethod == null)
            {
                throw new InvalidOperationException(
                    $"Property '{Property.DeclaringType?.Name}.{Property.Name}' is read-only.");
            }

            var convertedValue = TypeHelper.ConvertType(newValue, Property.PropertyType);
            Property.SetValue(resource, convertedValue);
        }
    }
}
