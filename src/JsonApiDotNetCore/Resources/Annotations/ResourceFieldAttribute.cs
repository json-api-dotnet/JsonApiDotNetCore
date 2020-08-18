using System;
using System.Reflection;

namespace JsonApiDotNetCore.Resources.Annotations
{
    /// <summary>
    /// Used to expose a resource property as a json:api field (attribute or relationship).
    /// </summary>
    public abstract class ResourceFieldAttribute : Attribute
    {
        /// <summary>
        /// The publicly exposed name of this json:api field.
        /// </summary>
        public string PublicName { get; internal set; }

        /// <summary>
        /// The resource property that this attribute is declared on.
        /// </summary>
        public PropertyInfo Property { get; internal set; }

        protected ResourceFieldAttribute()
        {
        }

        protected ResourceFieldAttribute(string publicName)
        {
            if (publicName != null && string.IsNullOrWhiteSpace(publicName))
            {
                throw new ArgumentException("Exposed name cannot be empty or contain only whitespace.",
                    nameof(publicName));
            }

            PublicName = publicName;
        }

        public override string ToString()
        {
            return PublicName ?? (Property != null ? Property.Name : base.ToString());
        }
    }
}
