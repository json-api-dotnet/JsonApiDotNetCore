using System;
using JsonApiDotNetCore.Internal;

namespace JsonApiDotNetCore.Models
{
    public class AttrAttribute : Attribute
    {
        /// <summary>
        /// Defines a public attribute exposed by the API
        /// </summary>
        /// 
        /// <param name="publicName">How this attribute is exposed through the API</param>
        /// <param name="isImmutable">Prevent PATCH requests from updating the value</param>
        /// <param name="isFilterable">Prevent filters on this attribute</param>
        /// <param name="isSortable">Prevent this attribute from being sorted by</param>
        /// 
        /// <example>
        /// 
        /// <code>
        /// public class Author : Identifiable
        /// {
        ///     [Attr("name")]
        ///     public string Name { get; set; }
        /// }
        /// </code>
        /// 
        /// </example>
        public AttrAttribute(string publicName, bool isImmutable = false, bool isFilterable = true, bool isSortable = true)
        {
            PublicAttributeName = publicName;
            IsImmutable = isImmutable;
            IsFilterable = isFilterable;
            IsSortable = isSortable;
        }

        public AttrAttribute(string publicName, string internalName, bool isImmutable = false)
        {
            PublicAttributeName = publicName;
            InternalAttributeName = internalName;
            IsImmutable = isImmutable;
        }

        /// <summary>
        /// How this attribute is exposed through the API
        /// </summary>
        public string PublicAttributeName { get; }

        /// <summary>
        /// The internal property name this attribute belongs to.
        /// </summary>
        public string InternalAttributeName { get; internal set; }

        /// <summary>
        /// Prevents PATCH requests from updating the value.
        /// </summary>
        public bool IsImmutable { get; }

        /// <summary>
        /// Whether or not this attribute can be filtered on via a query string filters.
        /// Attempts to filter on an attribute with `IsFilterable == false` will return
        /// an HTTP 400 response.
        /// </summary>
        public bool IsFilterable { get; }

        /// <summary>
        /// Whether or not this attribute can be sorted on via a query string sort.
        /// Attempts to filter on an attribute with `IsSortable == false` will return
        /// an HTTP 400 response.
        /// </summary>
        public bool IsSortable { get; }

        /// <summary>
        /// Get the value of the attribute for the given object.
        /// Returns null if the attribute does not belong to the
        /// provided object.
        /// </summary>
        public object GetValue(object entity)
        {
            return entity
                .GetType()
                .GetProperty(InternalAttributeName)
                ?.GetValue(entity);
        }

        /// <summary>
        /// Sets the value of the attribute on the given object.
        /// </summary>
        public void SetValue(object entity, object newValue)
        {
            var propertyInfo = entity
                .GetType()
                .GetProperty(InternalAttributeName);

            if (propertyInfo != null)
            {
                var convertedValue = TypeHelper.ConvertType(newValue, propertyInfo.PropertyType);

                propertyInfo.SetValue(entity, convertedValue);
            }
        }

        /// <summary>
        /// Whether or not the provided exposed name is equivalent to the one defined in on the model
        /// </summary>
        public virtual bool Is(string publicRelationshipName)
            => string.Equals(publicRelationshipName, PublicAttributeName, StringComparison.OrdinalIgnoreCase);
    }
}
