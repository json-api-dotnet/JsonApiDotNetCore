using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JsonApiDotNetCore.Resources.Annotations
{
    /// <summary>
    /// Used to expose a property on a resource class as a json:api to-many relationship (https://jsonapi.org/format/#document-resource-object-relationships)
    /// through a many-to-many join relationship.
    /// </summary>
    /// <example>
    /// In the following example, we expose a relationship named "tags"
    /// through the navigation property `ArticleTags`.
    /// The `Tags` property is decorated with `NotMapped` so that EF does not try
    /// to map this to a database relationship.
    /// <code><![CDATA[
    /// public sealed class Article : Identifiable
    /// {
    ///     [NotMapped]
    ///     [HasManyThrough(nameof(ArticleTags), PublicName = "tags")]
    ///     public ISet<Tag> Tags { get; set; }
    ///     public ISet<ArticleTag> ArticleTags { get; set; }
    /// }
    ///
    /// public class Tag : Identifiable
    /// {
    ///     [Attr]
    ///     public string Name { get; set; }
    /// }
    ///
    /// public sealed class ArticleTag
    /// {
    ///     public int ArticleId { get; set; }
    ///     public Article Article { get; set; }
    ///
    ///     public int TagId { get; set; }
    ///     public Tag Tag { get; set; }
    /// }
    /// ]]></code>
    /// </example>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class HasManyThroughAttribute : HasManyAttribute
    {
        /// <summary>
        /// The name of the join property on the parent resource.
        /// In the example described above, this would be "ArticleTags".
        /// </summary>
        public string ThroughPropertyName { get; }

        /// <summary>
        /// The join type.
        /// In the example described above, this would be `ArticleTag`.
        /// </summary>
        public Type ThroughType { get; internal set; }

        /// <summary>
        /// The navigation property back to the parent resource from the through type.
        /// In the example described above, this would point to the `Article.ArticleTags.Article` property.
        /// </summary>
        public PropertyInfo LeftProperty { get; internal set; }

        /// <summary>
        /// The ID property back to the parent resource from the through type.
        /// In the example described above, this would point to the `Article.ArticleTags.ArticleId` property.
        /// </summary>
        public PropertyInfo LeftIdProperty { get; internal set; }

        /// <summary>
        /// The navigation property to the related resource from the through type.
        /// In the example described above, this would point to the `Article.ArticleTags.Tag` property.
        /// </summary>
        public PropertyInfo RightProperty { get; internal set; }

        /// <summary>
        /// The ID property to the related resource from the through type.
        /// In the example described above, this would point to the `Article.ArticleTags.TagId` property.
        /// </summary>
        public PropertyInfo RightIdProperty { get; internal set; }

        /// <summary>
        /// The join resource property on the parent resource.
        /// In the example described above, this would point to the `Article.ArticleTags` property.
        /// </summary>
        public PropertyInfo ThroughProperty { get; internal set; }

        /// <summary>
        /// The internal navigation property path to the related resource.
        /// In the example described above, this would contain "ArticleTags.Tag".
        /// </summary>
        public override string RelationshipPath => $"{ThroughProperty.Name}.{RightProperty.Name}";

        /// <summary>
        /// Creates a HasMany relationship through a many-to-many join relationship.
        /// </summary>
        /// <param name="throughPropertyName">The name of the navigation property that will be used to access the join relationship.</param>
        public HasManyThroughAttribute(string throughPropertyName)
        {
            ThroughPropertyName = throughPropertyName ?? throw new ArgumentNullException(nameof(throughPropertyName));
        }

        /// <summary>
        /// Traverses through the provided resource and returns the value of the relationship on the other side of the through type.
        /// In the example described above, this would be the value of "Articles.ArticleTags.Tag".
        /// </summary>
        public override object GetValue(object resource)
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));

            IEnumerable throughEntities = (IEnumerable)ThroughProperty.GetValue(resource) ?? Array.Empty<object>();

            IEnumerable<object> rightResources = throughEntities
                .Cast<object>()
                .Select(te =>  RightProperty.GetValue(te));

            return TypeHelper.CopyToTypedCollection(rightResources, Property.PropertyType);
        }

        /// <summary>
        /// Traverses through the provided resource and sets the value of the relationship on the other side of the through type.
        /// In the example described above, this would be the value of "Articles.ArticleTags.Tag".
        /// </summary>
        public override void SetValue(object resource, object newValue)
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));

            base.SetValue(resource, newValue);

            if (newValue == null)
            {
                ThroughProperty.SetValue(resource, null);
            }
            else
            {
                List<object> throughResources = new List<object>();
                foreach (IIdentifiable identifiable in (IEnumerable)newValue)
                {
                    var throughResource = TypeHelper.CreateInstance(ThroughType);

                    LeftProperty.SetValue(throughResource, resource);
                    RightProperty.SetValue(throughResource, identifiable);
                    throughResources.Add(throughResource);
                }

                var typedCollection = TypeHelper.CopyToTypedCollection(throughResources, ThroughProperty.PropertyType);
                ThroughProperty.SetValue(resource, typedCollection);
            }
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

            var other = (HasManyThroughAttribute) obj;

            return ThroughPropertyName == other.ThroughPropertyName && ThroughType == other.ThroughType &&
                   LeftProperty == other.LeftProperty && LeftIdProperty == other.LeftIdProperty &&
                   RightProperty == other.RightProperty && RightIdProperty == other.RightIdProperty &&
                   ThroughProperty == other.ThroughProperty && base.Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ThroughPropertyName, ThroughType, LeftProperty, LeftIdProperty, RightProperty,
                RightIdProperty, ThroughProperty, base.GetHashCode());
        }
    }
}
