using System;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models.Links;

namespace JsonApiDotNetCore.Models.Annotation
{
    public abstract class RelationshipAttribute : ResourceFieldAttribute
    {
        public string InverseNavigation { get; internal set; }

        /// <summary>
        /// The internal navigation property path to the related entity.
        /// </summary>
        /// <remarks>
        /// In all cases except the HasManyThrough relationships, this will just be the property name.
        /// </remarks>
        public virtual string RelationshipPath => Property.Name;

        /// <summary>
        /// The related entity type. This does not necessarily match the navigation property type.
        /// In the case of a HasMany relationship, this value will be the generic argument type.
        /// </summary>
        /// <example>
        /// <code><![CDATA[
        /// public List<Tag> Tags { get; set; } // Type => Tag
        /// ]]></code>
        /// </example>
        public Type RightType { get; internal set; }

        /// <summary>
        /// The parent entity type. This is the type of the class in which this attribute was used.
        /// </summary>
        public Type LeftType { get; internal set; }

        /// <summary>
        /// Configures which links to show in the <see cref="RelationshipLinks"/>
        /// object for this relationship.
        /// </summary>
        public Link RelationshipLinks { get; }

        public bool CanInclude { get; }

        protected RelationshipAttribute(string publicName, Link relationshipLinks, bool canInclude)
            : base(publicName)
        {
            if (relationshipLinks == Link.Paging)
                throw new JsonApiSetupException($"{Link.Paging:g} not allowed for argument {nameof(relationshipLinks)}");

            RelationshipLinks = relationshipLinks;
            CanInclude = canInclude;
        }

        /// <summary>
        /// Gets the value of the resource property this attributes was declared on.
        /// </summary>
        public virtual object GetValue(object entity)
        {
            return Property.GetValue(entity);
        }

        /// <summary>
        /// Sets the value of the resource property this attributes was declared on.
        /// </summary>
        public virtual void SetValue(object entity, object newValue, IResourceFactory resourceFactory)
        {
            Property.SetValue(entity, newValue);
        }

        public override string ToString()
        {
            return base.ToString() + ":" + PublicName;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = (RelationshipAttribute) obj;

            return PublicName == other.PublicName && LeftType == other.LeftType &&
                   RightType == other.RightType;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PublicName, LeftType, RightType);
        }
    }
}
