using System;
using System.Reflection;
using JetBrains.Annotations;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace JsonApiDotNetCore.Resources.Annotations
{
    /// <summary>
    /// Used to expose a property on a resource class as a JSON:API field (attribute or relationship). See
    /// https://jsonapi.org/format/#document-resource-object-fields.
    /// </summary>
    [PublicAPI]
    public abstract class ResourceFieldAttribute : Attribute
    {
        private string _publicName;

        /// <summary>
        /// The publicly exposed name of this JSON:API field. When not explicitly assigned, the configured naming convention is applied on the property name.
        /// </summary>
        public string PublicName
        {
            get => _publicName;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Exposed name cannot be null, empty or contain only whitespace.", nameof(value));
                }

                _publicName = value;
            }
        }

        /// <summary>
        /// The resource property that this attribute is declared on.
        /// </summary>
        public PropertyInfo Property { get; internal set; }

        public override string ToString()
        {
            return PublicName ?? (Property != null ? Property.Name : base.ToString());
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

            var other = (ResourceFieldAttribute)obj;

            return PublicName == other.PublicName && Property == other.Property;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PublicName, Property);
        }
    }
}
