using System;
using System.Reflection;
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
        public AttrAttribute(string publicName = null, bool isImmutable = false, bool isFilterable = true, bool isSortable = true)
        {
            PublicAttributeName = publicName;
            IsImmutable = isImmutable;
            IsFilterable = isFilterable;
            IsSortable = isSortable;
        }

        /// <summary>
        /// Do not use this overload in your applications.
        /// Provides a method for instantiating instances of `AttrAttribute` and specifying
        /// the internal property name.
        /// The primary intent for this was to enable certain types of unit tests to be possible.
        /// This overload will be deprecated and removed in future releases and an alternative
        /// for unit tests will be provided.
        /// </summary>
        public AttrAttribute(string publicName, string internalName, bool isImmutable = false)
        {
            PublicAttributeName = publicName;
            InternalAttributeName = internalName;
            IsImmutable = isImmutable;
        }

        /// <summary>
        /// How this attribute is exposed through the API
        /// </summary>
        public string PublicAttributeName { get; internal set;}

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
        /// The member property info
        /// </summary>
        internal PropertyInfo PropertyInfo { get; set; }

        /// <summary>
        /// Get the value of the attribute for the given object.
        /// Returns null if the attribute does not belong to the
        /// provided object.
        /// </summary>
        public object GetValue(object entity)
        {
            if (entity == null)
                throw new InvalidOperationException("Cannot GetValue from null object.");

            var prop = GetResourceProperty(entity);
            return prop?.GetValue(entity);
        }

        /// <summary>
        /// Sets the value of the attribute on the given object.
        /// </summary>
        public void SetValue(object entity, object newValue)
        {
            if (entity == null)
                throw new InvalidOperationException("Cannot SetValue on null object.");

            var prop = GetResourceProperty(entity);
            if(prop != null)
            {
                var convertedValue = TypeHelper.ConvertType(newValue, prop.PropertyType);
                prop.SetValue(entity, convertedValue);
            }            
        }

        private PropertyInfo GetResourceProperty(object resource)
        {
            // There are some scenarios, especially ones where users are using a different
            // data model than view model, where they may use a repository implmentation
            // that does not match the deserialized type. For now, we will continue to support
            // this use case.
            var targetType = resource.GetType();
            if (targetType != PropertyInfo.DeclaringType)
            {
                var propertyInfo = resource
                    .GetType()
                    .GetProperty(InternalAttributeName);

                return propertyInfo;

                // TODO: this should throw but will be a breaking change in some cases
                //if (propertyInfo == null)
                //    throw new InvalidOperationException(
                //        $"'{targetType}' does not contain a member named '{InternalAttributeName}'." +
                //        $"There is also a mismatch in target types. Expected '{PropertyInfo.DeclaringType}' but instead received '{targetType}'.");
            }

            return PropertyInfo;
        }

        /// <summary>
        /// Whether or not the provided exposed name is equivalent to the one defined in on the model
        /// </summary>
        public virtual bool Is(string publicRelationshipName)
            => string.Equals(publicRelationshipName, PublicAttributeName, StringComparison.OrdinalIgnoreCase);
    }
}
