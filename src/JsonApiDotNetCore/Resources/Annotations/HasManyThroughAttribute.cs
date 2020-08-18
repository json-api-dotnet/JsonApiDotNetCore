using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JsonApiDotNetCore.Resources.Annotations
{
    /// <summary>
    /// Create a HasMany relationship through a many-to-many join relationship.
    /// This type can only be applied on types that implement ICollection.
    /// </summary>
    /// 
    /// <example>
    /// In the following example, we expose a relationship named "tags"
    /// through the navigation property `ArticleTags`.
    /// The `Tags` property is decorated with `NotMapped` so that EF does not try
    /// to map this to a database relationship.
    /// <code><![CDATA[
    /// [NotMapped]
    /// [HasManyThrough("tags", nameof(ArticleTags))]
    /// public ICollection<Tag> Tags { get; set; }
    /// public ICollection<ArticleTag> ArticleTags { get; set; }
    /// ]]></code>
    /// </example>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class HasManyThroughAttribute : HasManyAttribute
    {
        /// <summary>
        /// The name of the join property on the parent resource.
        /// </summary>
        /// <example>
        /// In the `[HasManyThrough("tags", nameof(ArticleTags))]` example
        /// this would be "ArticleTags".
        /// </example>
        internal string ThroughPropertyName { get; }

        /// <summary>
        /// The join type.
        /// </summary>
        /// <example>
        /// In the `[HasManyThrough("tags", nameof(ArticleTags))]` example
        /// this would be `ArticleTag`.
        /// </example>
        public Type ThroughType { get; internal set; }

        /// <summary>
        /// The navigation property back to the parent resource from the through type.
        /// </summary>
        /// 
        /// <example>
        /// In the `[HasManyThrough("tags", nameof(ArticleTags))]` example
        /// this would point to the `Article.ArticleTags.Article` property
        ///
        /// <code>
        /// public Article Article { get; set; }
        /// </code>
        ///
        /// </example>
        public PropertyInfo LeftProperty { get; internal set; }

        /// <summary>
        /// The id property back to the parent resource from the through type.
        /// </summary>
        /// 
        /// <example>
        /// In the `[HasManyThrough("tags", nameof(ArticleTags))]` example
        /// this would point to the `Article.ArticleTags.ArticleId` property
        ///
        /// <code>
        /// public int ArticleId { get; set; }
        /// </code>
        ///
        /// </example>
        public PropertyInfo LeftIdProperty { get; internal set; }

        /// <summary>
        /// The navigation property to the related resource from the through type.
        /// </summary>
        /// 
        /// <example>
        /// In the `[HasManyThrough("tags", nameof(ArticleTags))]` example
        /// this would point to the `Article.ArticleTags.Tag` property
        ///
        /// <code>
        /// public Tag Tag { get; set; }
        /// </code>
        ///
        /// </example>
        public PropertyInfo RightProperty { get; internal set; }

        /// <summary>
        /// The id property to the related resource from the through type.
        /// </summary>
        /// 
        /// <example>
        /// In the `[HasManyThrough("tags", nameof(ArticleTags))]` example
        /// this would point to the `Article.ArticleTags.TagId` property
        ///
        /// <code>
        /// public int TagId { get; set; }
        /// </code>
        ///
        /// </example>
        public PropertyInfo RightIdProperty { get; internal set; }

        /// <summary>
        /// The join resource property on the parent resource.
        /// </summary>
        /// 
        /// <example>
        /// In the `[HasManyThrough("tags", nameof(ArticleTags))]` example
        /// this would point to the `Article.ArticleTags` property
        ///
        /// <code><![CDATA[
        /// public ICollection<ArticleTags> ArticleTags { get; set; }
        /// ]]></code>
        ///
        /// </example>
        public PropertyInfo ThroughProperty { get; internal set; }

        /// <inheritdoc />
        /// <example>
        /// "ArticleTags.Tag"
        /// </example>
        public override string RelationshipPath => $"{ThroughProperty.Name}.{RightProperty.Name}";

        /// <summary>
        /// Create a HasMany relationship through a many-to-many join relationship.
        /// The public name exposed through the API will be based on the configured convention.
        /// </summary>
        /// 
        /// <param name="throughPropertyName">The name of the navigation property that will be used to get the HasMany relationship</param>
        /// <param name="relationshipLinks">Which links are available. Defaults to <see cref="Links.All"/></param>
        /// <param name="canInclude">Whether or not this relationship can be included using the <c>?include=public-name</c> query string</param>
        /// 
        /// <example>
        /// <code>
        /// [HasManyThrough(nameof(ArticleTags), relationshipLinks: Links.All, canInclude: true)]
        /// </code>
        /// </example>
        public HasManyThroughAttribute(string throughPropertyName, Links relationshipLinks = Links.All, bool canInclude = true)
        : base(null, relationshipLinks, canInclude)
        {
            ThroughPropertyName = throughPropertyName;
        }

        /// <summary>
        /// Create a HasMany relationship through a many-to-many join relationship.
        /// </summary>
        /// 
        /// <param name="publicName">The relationship name as exposed by the API</param>
        /// <param name="throughPropertyName">The name of the navigation property that will be used to get the HasMany relationship</param>
        /// <param name="relationshipLinks">Which links are available. Defaults to <see cref="Links.All"/></param>
        /// <param name="canInclude">Whether or not this relationship can be included using the <c>?include=public-name</c> query string</param>
        /// 
        /// <example>
        /// <code>
        /// [HasManyThrough("tags", nameof(ArticleTags), relationshipLinks: Links.All, canInclude: true)]
        /// </code>
        /// </example>
        public HasManyThroughAttribute(string publicName, string throughPropertyName, Links relationshipLinks = Links.All, bool canInclude = true)
        : base(publicName, relationshipLinks, canInclude)
        {
            ThroughPropertyName = throughPropertyName;
        }

        /// <summary>
        /// Traverses through the provided resource and returns the 
        /// value of the relationship on the other side of a through type
        /// (e.g. Articles.ArticleTags.Tag).
        /// </summary>
        public override object GetValue(object resource)
        {
            IEnumerable throughResources = (IEnumerable)ThroughProperty.GetValue(resource) ?? Array.Empty<object>();

            IEnumerable<object> rightResources = throughResources
                .Cast<object>()
                .Select(rightResource =>  RightProperty.GetValue(rightResource));

            return TypeHelper.CopyToTypedCollection(rightResources, Property.PropertyType);
        }

        /// <inheritdoc />
        public override void SetValue(object resource, object newValue, IResourceFactory resourceFactory)
        {
            base.SetValue(resource, newValue, resourceFactory);

            if (newValue == null)
            {
                ThroughProperty.SetValue(resource, null);
            }
            else
            {
                List<object> throughResources = new List<object>();
                foreach (IIdentifiable identifiable in (IEnumerable)newValue)
                {
                    object throughResource = resourceFactory.CreateInstance(ThroughType);
                    LeftProperty.SetValue(throughResource, resource);
                    RightProperty.SetValue(throughResource, identifiable);
                    throughResources.Add(throughResource);
                }

                var typedCollection = TypeHelper.CopyToTypedCollection(throughResources, ThroughProperty.PropertyType);
                ThroughProperty.SetValue(resource, typedCollection);
            }
        }
    }
}
