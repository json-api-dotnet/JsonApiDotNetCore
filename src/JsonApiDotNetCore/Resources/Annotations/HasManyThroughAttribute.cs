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

            // if (resource == null) throw new ArgumentNullException(nameof(resource));
            //
            // // TODO: Passing null for the resourceFactory parameter is wrong here. Instead, GetManyValue() should properly throw when null is passed in.
            // return GetManyValue(resource);
            
            // The resouceFactory argument needs to be an optional param independent of this method calling it.
            // In should actually be the responsibility of the relationship attribute to know whether to use the resource factory or not,
            // instead of the caller passing it along. But this is hard because we're working with attributes rather than having a meta abstraction / service
            // We can consider working around it with a static internal setter.

            if (resource == null) throw new ArgumentNullException(nameof(resource));

            IEnumerable throughEntities = (IEnumerable)ThroughProperty.GetValue(resource) ?? Array.Empty<object>();

            IEnumerable<object> rightResources = throughEntities
                .Cast<object>()
                .Select(te =>  RightProperty.GetValue(te));

            return TypeHelper.CopyToTypedCollection(rightResources, Property.PropertyType);
            
            
        }

        internal override IEnumerable<IIdentifiable> GetManyValue(object resource, IResourceFactory resourceFactory = null)
        {
            // TODO: This method contains surprising code: Instead of returning the contents of a collection,
            // it modifies data and performs logic that is highly specific to what EntityFrameworkCoreRepository needs.
            //     => We cannot around this logic and data modification: we must perform a transformation of this collection before returning it.
            //     The added bit is only an extension of this. It is not EF Core specific but JADNC specific.
            //     I think it is relevant because only including Article.ArticleTag rather than Article.ArticleTag.Tag is the equivalent
            //     of having a primary ID only projection on the secondary resource.
            // This method is not reusable at all, it should not be concerned if resources are loaded, so should be moved into the caller instead.
            //     => There are already some cases of it being reused
            // After moving the code, the unneeded copying into new collections multiple times can be removed too.
            //     => I don't think we can. There is no guarantee that a dev uses the same collection type for the join entities and right resource collections.
            
            if (resource == null) throw new ArgumentNullException(nameof(resource));

            var value = ThroughProperty.GetValue(resource);

            var throughEntities = value == null ? Array.Empty<object>() : ((IEnumerable)value).Cast<object>().ToArray();
            var rightResourcesAreLoaded =  throughEntities.Any() && RightProperty.GetValue(throughEntities.First()) != null;
            
            // Even if the right resources aren't loaded, we can still construct identifier objects using the ID set on the through entity.
            var rightResources = rightResourcesAreLoaded
                ? throughEntities.Select(e => RightProperty.GetValue(e)).Cast<IIdentifiable>()
                : throughEntities.Select(e => CreateRightResourceWithId(e, resourceFactory));

            return (IEnumerable<IIdentifiable>)TypeHelper.CopyToTypedCollection(rightResources, Property.PropertyType);
        }

        private IIdentifiable CreateRightResourceWithId(object throughEntity, IResourceFactory resourceFactory)
        {
            if (resourceFactory == null) throw new ArgumentNullException(nameof(resourceFactory));

            var rightResource = resourceFactory.CreateInstance(RightType);
            rightResource.StringId = RightIdProperty.GetValue(throughEntity)!.ToString();

            return rightResource;
        }

        /// <summary>
        /// Traverses through the provided resource and sets the value of the relationship on the other side of the through type.
        /// In the example described above, this would be the value of "Articles.ArticleTags.Tag".
        /// </summary>
        public override void SetValue(object resource, object newValue, IResourceFactory resourceFactory)
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));
            if (resourceFactory == null) throw new ArgumentNullException(nameof(resourceFactory));

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
                    var throughResource = TypeHelper.IsOrImplementsInterface(ThroughType, typeof(IIdentifiable))
                        ? resourceFactory.CreateInstance(ThroughType)
                        : TypeHelper.CreateInstance(ThroughType);

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
