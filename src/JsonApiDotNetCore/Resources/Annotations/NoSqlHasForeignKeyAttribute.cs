using System;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Resources.Annotations
{
    /// <summary>
    /// Used to provide additional information for a JSON:API relationship with a foreign key
    /// (https://jsonapi.org/format/#document-resource-object-relationships).
    /// </summary>
    [PublicAPI]
    [AttributeUsage(AttributeTargets.Property)]
    public class NoSqlHasForeignKeyAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of the foreign key property corresponding to the annotated navigation property.
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the navigation property is on the dependent side of the foreign key relationship. The default is
        /// <see langword="true" />.
        /// </summary>
        public bool IsDependent { get; set; } = true;

        public NoSqlHasForeignKeyAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }
    }
}
