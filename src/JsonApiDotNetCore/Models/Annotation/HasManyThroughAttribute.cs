using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models.Links;

namespace JsonApiDotNetCore.Models
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
    /// [HasManyThrough("tags", nameof(ArticleTags))]
    /// public List&lt;Tag&gt; Tags { get; set; }
    /// public List&lt;ArticleTag&gt; ArticleTags { get; set; }
    /// </code>
    /// </example>
    public class HasManyThroughAttribute : HasManyAttribute
    {
        /// <summary>
        /// Create a HasMany relationship through a many-to-many join relationship.
        /// The public name exposed through the API will be based on the configured convention.
        /// </summary>
        /// 
        /// <param name="internalThroughName">The name of the navigation property that will be used to get the HasMany relationship</param>
        /// <param name="relationshipLinks">Which links are available. Defaults to <see cref="Link.All"/></param>
        /// <param name="canInclude">Whether or not this relationship can be included using the <c>?include=public-name</c> query string</param>
        /// <param name="mappedBy">The name of the entity mapped property, defaults to null</param>
        /// 
        /// <example>
        /// <code>
        /// [HasManyThrough(nameof(ArticleTags), documentLinks: Link.All, canInclude: true)]
        /// </code>
        /// </example>
        public HasManyThroughAttribute(string internalThroughName, Link relationshipLinks = Link.All, bool canInclude = true, string mappedBy = null)
        : base(null, relationshipLinks, canInclude, mappedBy)
        {
            InternalThroughName = internalThroughName;
        }

        /// <summary>
        /// Create a HasMany relationship through a many-to-many join relationship.
        /// </summary>
        /// 
        /// <param name="publicName">The relationship name as exposed by the API</param>
        /// <param name="internalThroughName">The name of the navigation property that will be used to get the HasMany relationship</param>
        /// <param name="documentLinks">Which links are available. Defaults to <see cref="Link.All"/></param>
        /// <param name="canInclude">Whether or not this relationship can be included using the <c>?include=public-name</c> query string</param>
        /// <param name="mappedBy">The name of the entity mapped property, defaults to null</param>
        /// 
        /// <example>
        /// <code>
        /// [HasManyThrough("tags", nameof(ArticleTags), documentLinks: Link.All, canInclude: true)]
        /// </code>
        /// </example>
        public HasManyThroughAttribute(string publicName, string internalThroughName, Link documentLinks = Link.All, bool canInclude = true, string mappedBy = null)
        : base(publicName, documentLinks, canInclude, mappedBy)
        {
            InternalThroughName = internalThroughName;
        }

        /// <summary>
        /// Traverses the through the provided entity and returns the 
        /// value of the relationship on the other side of a join entity
        /// (e.g. Articles.ArticleTags.Tag).
        /// </summary>
        public override object GetValue(object entity)
        {
            var throughNavigationProperty = entity.GetType()
                                        .GetProperties()
                                        .SingleOrDefault(p => string.Equals(p.Name, InternalThroughName, StringComparison.OrdinalIgnoreCase));

            var throughEntities = throughNavigationProperty.GetValue(entity);

            if (throughEntities == null)
                // return an empty list for the right-type of the property.
                return TypeHelper.CreateListFor(DependentType);

            // the right entities are included on the navigation/through entities. Extract and return them.
            var rightEntities = new List<IIdentifiable>();
            foreach (var rightEntity in (IList)throughEntities)
                rightEntities.Add((IIdentifiable)RightProperty.GetValue(rightEntity));

            return rightEntities.Cast(DependentType);
        }


        /// <summary>
        /// Sets the value of the property identified by this attribute
        /// </summary>
        /// <param name="entity">The target object</param>
        /// <param name="newValue">The new property value</param>
        public override void SetValue(object entity, object newValue)
        {

            var propertyInfo = entity
                .GetType()
                .GetProperty(InternalRelationshipName);
            propertyInfo.SetValue(entity, newValue);

            if (newValue == null)
            {
                ThroughProperty.SetValue(entity, null);
            }
            else
            {
                var throughRelationshipCollection = (IList)Activator.CreateInstance(ThroughProperty.PropertyType);
                ThroughProperty.SetValue(entity, throughRelationshipCollection);

                foreach (IIdentifiable pointer in (IList)newValue)
                {
                    var throughInstance = Activator.CreateInstance(ThroughType);
                    LeftProperty.SetValue(throughInstance, entity);
                    RightProperty.SetValue(throughInstance, pointer);
                    throughRelationshipCollection.Add(throughInstance);
                }
            }
        }

        /// <summary>
        /// The name of the join property on the parent resource.
        /// </summary>
        /// <example>
        /// In the `[HasManyThrough("tags", nameof(ArticleTags))]` example
        /// this would be "ArticleTags".
        /// </example>
        public string InternalThroughName { get; private set; }

        /// <summary>
        /// The join type.
        /// </summary>
        /// <example>
        /// In the `[HasManyThrough("tags", nameof(ArticleTags))]` example
        /// this would be `ArticleTag`.
        /// </example>
        public Type ThroughType { get; internal set; }

        /// <summary>
        /// The navigation property back to the parent resource from the join type.
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
        /// The id property back to the parent resource from the join type.
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
        /// The navigation property to the related resource from the join type.
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
        /// The id property to the related resource from the join type.
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
        /// The join entity property on the parent resource.
        /// </summary>
        /// 
        /// <example>
        /// In the `[HasManyThrough("tags", nameof(ArticleTags))]` example
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
