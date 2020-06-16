using System;
using System.Reflection;
using JsonApiDotNetCore.Internal;

namespace JsonApiDotNetCore.Models
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class AttrAttribute : Attribute, IResourceField
    {
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
        {
            if (publicName == null)
            {
                throw new ArgumentNullException(nameof(publicName));
            }

            if (string.IsNullOrWhiteSpace(publicName))
            {
                throw new ArgumentException("Exposed name cannot be empty or contain only whitespace.", nameof(publicName));
            }

            PublicAttributeName = publicName;
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
        public AttrAttribute(string publicName, AttrCapabilities capabilities) : this(publicName)
        {
            HasExplicitCapabilities = true;
            Capabilities = capabilities;
        }

        string IResourceField.PropertyName => PropertyInfo.Name;

        /// <summary>
        /// The publicly exposed name of this json:api attribute.
        /// </summary>
        public string PublicAttributeName { get; internal set; }

        internal bool HasExplicitCapabilities { get; }
        public AttrCapabilities Capabilities { get; internal set; }

        /// <summary>
        /// The resource property that this attribute is declared on.
        /// </summary>
        public PropertyInfo PropertyInfo { get; internal set; }

        /// <summary>
        /// Get the value of the attribute for the given object.
        /// Returns null if the attribute does not belong to the
        /// provided object.
        /// </summary>
        public object GetValue(object entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (PropertyInfo.GetMethod == null)
            {
                throw new InvalidOperationException($"Property '{PropertyInfo.DeclaringType?.Name}.{PropertyInfo.Name}' is write-only.");
            }

            return PropertyInfo.GetValue(entity);
        }

        /// <summary>
        /// Sets the value of the attribute on the given object.
        /// </summary>
        public void SetValue(object entity, object newValue)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (PropertyInfo.SetMethod == null)
            {
                throw new InvalidOperationException(
                    $"Property '{PropertyInfo.DeclaringType?.Name}.{PropertyInfo.Name}' is read-only.");
            }

            var convertedValue = TypeHelper.ConvertType(newValue, PropertyInfo.PropertyType);
            PropertyInfo.SetValue(entity, convertedValue);
        }

        /// <summary>
        /// Whether or not the provided exposed name is equivalent to the one defined in on the model
        /// </summary>
        public bool Is(string publicRelationshipName) => publicRelationshipName == PublicAttributeName;
    }
}
