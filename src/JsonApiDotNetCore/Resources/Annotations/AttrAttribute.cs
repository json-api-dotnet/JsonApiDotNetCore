using System;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Resources.Annotations
{
    /// <summary>
    /// Used to expose a property on a resource class as a JSON:API attribute (https://jsonapi.org/format/#document-resource-object-attributes).
    /// </summary>
    [PublicAPI]
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class AttrAttribute : ResourceFieldAttribute
    {
        private AttrCapabilities? _capabilities;

        internal bool HasExplicitCapabilities => _capabilities != null;

        /// <summary>
        /// The set of capabilities that are allowed to be performed on this attribute. When not explicitly assigned, the configured default set of capabilities
        /// is used.
        /// </summary>
        /// <example>
        /// <code>
        /// public class Author : Identifiable
        /// {
        ///     [Attr(Capabilities = AttrCapabilities.AllowFilter | AttrCapabilities.AllowSort)]
        ///     public string Name { get; set; }
        /// }
        /// </code>
        /// </example>
        public AttrCapabilities Capabilities
        {
            get => _capabilities ?? default;
            set => _capabilities = value;
        }

        /// <summary>
        /// Get the value of the attribute for the given object. Throws if the attribute does not belong to the provided object.
        /// </summary>
        public object GetValue(object resource)
        {
            ArgumentGuard.NotNull(resource, nameof(resource));

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
            ArgumentGuard.NotNull(resource, nameof(resource));

            if (Property.SetMethod == null)
            {
                throw new InvalidOperationException($"Property '{Property.DeclaringType?.Name}.{Property.Name}' is read-only.");
            }

            Property.SetValue(resource, newValue);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = (AttrAttribute)obj;

            return Capabilities == other.Capabilities && base.Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Capabilities, base.GetHashCode());
        }
    }
}
