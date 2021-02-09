using System;
using System.Reflection;
using JsonApiDotNetCore.Configuration;

namespace JsonApiDotNetCore.Resources.Annotations
{
    /// <summary>
    /// Used to expose a property on a resource class as a JSON:API relationship (https://jsonapi.org/format/#document-resource-object-relationships).
    /// </summary>
    public abstract class RelationshipAttribute : ResourceFieldAttribute
    {
        /// <summary>
        /// The property name of the EF Core inverse navigation, which may or may not exist.
        /// Even if it exists, it may not be exposed as a JSON:API relationship.
        /// </summary>
        /// <example>
        /// <code><![CDATA[
        /// public class Article : Identifiable
        /// {
        ///     [HasOne] // InverseNavigationProperty = Person.Articles
        ///     public Person Owner { get; set; }
        /// }
        /// 
        /// public class Person : Identifiable
        /// {
        ///     [HasMany] // InverseNavigationProperty = Article.Owner
        ///     public ICollection<Article> Articles { get; set; }
        /// }
        /// ]]></code>
        /// </example>
        public PropertyInfo InverseNavigationProperty { get; set; }

        /// <summary>
        /// The internal navigation property path to the related resource.
        /// </summary>
        /// <remarks>
        /// In all cases except for <see cref="HasManyThroughAttribute"/> relationships, this equals the property name.
        /// </remarks>
        public virtual string RelationshipPath => Property.Name;

        /// <summary>
        /// The child resource type. This does not necessarily match the navigation property type.
        /// In the case of a <see cref="HasManyAttribute"/> relationship, this value will be the collection element type.
        /// </summary>
        /// <example>
        /// <code><![CDATA[
        /// public List<Tag> Tags { get; set; } // Type => Tag
        /// ]]></code>
        /// </example>
        public Type RightType { get; internal set; }

        /// <summary>
        /// The parent resource type. This is the type of the class in which this attribute was used.
        /// </summary>
        public Type LeftType { get; internal set; }

        /// <summary>
        /// Configures which links to show in the <see cref="Serialization.Objects.RelationshipLinks"/>
        /// object for this relationship.
        /// Defaults to <see cref="LinkTypes.NotConfigured"/>, which falls back to <see cref="ResourceLinksAttribute.RelationshipLinks"/>
        /// and then falls back to <see cref="IJsonApiOptions.RelationshipLinks"/>.
        /// </summary>
        public LinkTypes Links { get; set; } = LinkTypes.NotConfigured;

        /// <summary>
        /// Whether or not this relationship can be included using the <c>?include=publicName</c> query string parameter.
        /// This is <c>true</c> by default.
        /// </summary>
        public bool CanInclude { get; set; } = true;

        /// <summary>
        /// Gets the value of the resource property this attribute was declared on.
        /// </summary>
        public virtual object GetValue(object resource)
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));

            return Property.GetValue(resource);
        }

        /// <summary>
        /// Sets the value of the resource property this attribute was declared on.
        /// </summary>
        public virtual void SetValue(object resource, object newValue)
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));

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

            var other = (RelationshipAttribute) obj;

            return LeftType == other.LeftType && RightType == other.RightType && Links == other.Links && 
                CanInclude == other.CanInclude && base.Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(LeftType, RightType, Links, CanInclude, base.GetHashCode());
        }
    }
}
