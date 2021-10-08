using System;
using System.Reflection;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace JsonApiDotNetCore.Resources.Annotations
{
    /// <summary>
    /// Used to expose a property on a resource class as a JSON:API relationship (https://jsonapi.org/format/#document-resource-object-relationships).
    /// </summary>
    [PublicAPI]
    public abstract class RelationshipAttribute : ResourceFieldAttribute
    {
        private protected static readonly CollectionConverter CollectionConverter = new();

        /// <summary>
        /// The CLR type in which this relationship is declared.
        /// </summary>
        internal Type LeftClrType { get; set; }

        /// <summary>
        /// The CLR type this relationship points to. In the case of a <see cref="HasManyAttribute" /> relationship, this value will be the collection element
        /// type.
        /// </summary>
        /// <example>
        /// <code><![CDATA[
        /// public ISet<Tag> Tags { get; set; } // RightClrType: typeof(Tag)
        /// ]]></code>
        /// </example>
        internal Type RightClrType { get; set; }

        /// <summary>
        /// The <see cref="PropertyInfo" /> of the EF Core inverse navigation, which may or may not exist. Even if it exists, it may not be exposed as a JSON:API
        /// relationship.
        /// </summary>
        /// <example>
        /// <code><![CDATA[
        /// public class Article : Identifiable
        /// {
        ///     [HasOne] // InverseNavigationProperty: Person.Articles
        ///     public Person Owner { get; set; }
        /// }
        /// 
        /// public class Person : Identifiable
        /// {
        ///     [HasMany] // InverseNavigationProperty: Article.Owner
        ///     public ICollection<Article> Articles { get; set; }
        /// }
        /// ]]></code>
        /// </example>
        public PropertyInfo InverseNavigationProperty { get; set; }

        /// <summary>
        /// The containing resource type in which this relationship is declared.
        /// </summary>
        public ResourceType LeftType { get; internal set; }

        /// <summary>
        /// The resource type this relationship points to. In the case of a <see cref="HasManyAttribute" /> relationship, this value will be the collection
        /// element type.
        /// </summary>
        public ResourceType RightType { get; internal set; }

        /// <summary>
        /// Configures which links to show in the <see cref="Serialization.Objects.RelationshipLinks" /> object for this relationship. Defaults to
        /// <see cref="LinkTypes.NotConfigured" />, which falls back to <see cref="ResourceLinksAttribute.RelationshipLinks" /> and then falls back to
        /// <see cref="IJsonApiOptions.RelationshipLinks" />.
        /// </summary>
        public LinkTypes Links { get; set; } = LinkTypes.NotConfigured;

        /// <summary>
        /// Whether or not this relationship can be included using the
        /// <c>
        /// ?include=publicName
        /// </c>
        /// query string parameter. This is <c>true</c> by default.
        /// </summary>
        public bool CanInclude { get; set; } = true;

        /// <summary>
        /// Gets the value of the resource property this attribute was declared on.
        /// </summary>
        public object GetValue(object resource)
        {
            ArgumentGuard.NotNull(resource, nameof(resource));

            return Property.GetValue(resource);
        }

        /// <summary>
        /// Sets the value of the resource property this attribute was declared on.
        /// </summary>
        public void SetValue(object resource, object newValue)
        {
            ArgumentGuard.NotNull(resource, nameof(resource));

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

            var other = (RelationshipAttribute)obj;

            return LeftClrType == other.LeftClrType && RightClrType == other.RightClrType && Links == other.Links && CanInclude == other.CanInclude &&
                base.Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(LeftClrType, RightClrType, Links, CanInclude, base.GetHashCode());
        }
    }
}
