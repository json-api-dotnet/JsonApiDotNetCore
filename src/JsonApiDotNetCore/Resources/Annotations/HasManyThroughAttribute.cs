using System;
using System.Reflection;

namespace JsonApiDotNetCore.Resources.Annotations
{
    /// <summary>
    /// Create a HasMany relationship through a many-to-many join relationship.
    /// This type can only be applied on types that implement IList.
    /// </summary>
    ///
    /// <example>
    /// In the following example, we expose a relationship named "tags"
    /// through the navigation property `ArticleTags`.
    /// The `Tags` property is decorated as `NotMapped` so that EF does not try
    /// to map this to a database relationship.
    /// <code>
    /// [NotMapped]
    /// [HasManyThrough(PublicName = "tags", nameof(ArticleTags))]
    /// public List&lt;Tag&gt; Tags { get; set; }
    /// public List&lt;ArticleTag&gt; ArticleTags { get; set; }
    /// </code>
    /// </example>
    public class HasManyThroughAttribute : HasManyAttribute
    {
        
        /// <summary>
        /// Creates a HasMany relationship through a many-to-many join relationship.
        /// </summary>
        /// <param name="throughPropertyName">
        /// The name of the navigation property that will be used to access the join relationship.
        /// </param>
        public HasManyThroughAttribute(string throughPropertyName)
        {
            InternalThroughName = throughPropertyName;
        }
        
        // /// <summary>
        // /// Create a HasMany relationship through a many-to-many join relationship.
        // /// The public name exposed through the API will be based on the configured convention.
        // /// </summary>
        // ///
        // /// <param name="internalThroughName">The name of the navigation property that will be used to get the HasMany relationship</param>
        // /// <param name="documentLinks">Which links are available. Defaults to <see cref="LinkTypes.All"/></param>
        // /// <param name="canInclude">Whether or not this relationship can be included using the <c>?include=public-name</c> query string</param>
        // /// <param name="mappedBy">The name of the entity mapped property, defaults to null</param>
        // ///
        // /// <example>
        // /// <code>
        // /// [HasManyThrough(nameof(ArticleTags), documentLinks: Link.All, canInclude: true)]
        // /// </code>
        // /// </example>
        // public HasManyThroughAttribute(string internalThroughName, LinkTypes documentLinks = LinkTypes.All, bool canInclude = true, string mappedBy = null)
        // : base(null, documentLinks, canInclude, mappedBy)
        // {
        //     InternalThroughName = internalThroughName;
        // }

        // /// <summary>
        // /// Create a HasMany relationship through a many-to-many join relationship.
        // /// </summary>
        // ///
        // /// <param name="publicName">The relationship name as exposed by the API</param>
        // /// <param name="internalThroughName">The name of the navigation property that will be used to get the HasMany relationship</param>
        // /// <param name="documentLinks">Which links are available. Defaults to <see cref="LinkTypes.All"/></param>
        // /// <param name="canInclude">Whether or not this relationship can be included using the <c>?include=public-name</c> query string</param>
        // /// <param name="mappedBy">The name of the entity mapped property, defaults to null</param>
        // ///
        // /// <example>
        // /// <code>
        // /// [HasManyThrough(PublicName = "tags", nameof(ArticleTags), documentLinks: Link.All, canInclude: true)]
        // /// </code>
        // /// </example>
        // public HasManyThroughAttribute(string publicName, string internalThroughName, LinkTypes documentLinks = LinkTypes.All, bool canInclude = true, string mappedBy = null)
        // : base(publicName, documentLinks, canInclude, mappedBy)
        // {
        //     InternalThroughName = internalThroughName;
        // }

        /// <summary>
        /// The name of the join property on the parent resource.
        /// </summary>
        /// <example>
        /// In the `[HasManyThrough(PublicName = "tags", nameof(ArticleTags))]` example
        /// this would be "ArticleTags".
        /// </example>
        public string InternalThroughName { get; private set; }

        /// <summary>
        /// The join type.
        /// </summary>
        /// <example>
        /// In the `[HasManyThrough(PublicName = "tags", nameof(ArticleTags))]` example
        /// this would be `ArticleTag`.
        /// </example>
        public Type ThroughType { get; internal set; }

        /// <summary>
        /// The navigation property back to the parent resource from the join type.
        /// </summary>
        ///
        /// <example>
        /// In the `[HasManyThrough(PublicName = "tags", nameof(ArticleTags))]` example
        /// this would point to the `Article.ArticleTags.Article` property
        ///
        /// <code>
        /// public Article Article { get; set; }
        /// </code>
        ///
        /// </example>
        public PropertyInfo LeftProperty { get; internal set; }

        /// <summary>
        /// The id property back to the parent resource from the join type.
        /// </summary>
        ///
        /// <example>
        /// In the `[HasManyThrough(PublicName = "tags", nameof(ArticleTags))]` example
        /// this would point to the `Article.ArticleTags.ArticleId` property
        ///
        /// <code>
        /// public int ArticleId { get; set; }
        /// </code>
        ///
        /// </example>
        public PropertyInfo LeftIdProperty { get; internal set; }

        /// <summary>
        /// The navigation property to the related resource from the join type.
        /// </summary>
        ///
        /// <example>
        /// In the `[HasManyThrough(PublicName = "tags", nameof(ArticleTags))]` example
        /// this would point to the `Article.ArticleTags.Tag` property
        ///
        /// <code>
        /// public Tag Tag { get; set; }
        /// </code>
        ///
        /// </example>
        public PropertyInfo RightProperty { get; internal set; }

        /// <summary>
        /// The id property to the related resource from the join type.
        /// </summary>
        ///
        /// <example>
        /// In the `[HasManyThrough(PublicName = "tags", nameof(ArticleTags))]` example
        /// this would point to the `Article.ArticleTags.TagId` property
        ///
        /// <code>
        /// public int TagId { get; set; }
        /// </code>
        ///
        /// </example>
        public PropertyInfo RightIdProperty { get; internal set; }

        /// <summary>
        /// The join entity property on the parent resource.
        /// </summary>
        ///
        /// <example>
        /// In the `[HasManyThrough(PublicName = "tags", nameof(ArticleTags))]` example
        /// this would point to the `Article.ArticleTags` property
        ///
        /// <code>
        /// public List&lt;ArticleTags&gt; ArticleTags { get; set; }
        /// </code>
        ///
        /// </example>
        public PropertyInfo ThroughProperty { get; internal set; }

        /// <inheritdoc />
        /// <example>
        /// "ArticleTags.Tag"
        /// </example>
        public override string RelationshipPath => $"{InternalThroughName}.{RightProperty.Name}";
    }
}
