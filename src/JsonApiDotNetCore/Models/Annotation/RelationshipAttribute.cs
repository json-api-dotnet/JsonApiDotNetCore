using System;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models.Links;

namespace JsonApiDotNetCore.Models
{
    public abstract class RelationshipAttribute : Attribute, IResourceField
    {
        protected RelationshipAttribute(string publicName, Link relationshipLinks, bool canInclude, string mappedBy)
        {
            if (relationshipLinks == Link.Paging)
                throw new JsonApiSetupException($"{Link.Paging.ToString("g")} not allowed for argument {nameof(relationshipLinks)}");

            PublicRelationshipName = publicName;
            RelationshipLinks = relationshipLinks;
            CanInclude = canInclude;
            EntityPropertyName = mappedBy;
        }

        public string ExposedInternalMemberName => InternalRelationshipName;
        public string PublicRelationshipName { get; internal set; }
        public string InternalRelationshipName { get; internal set; }
        public string InverseNavigation { get; internal set; }

        /// <summary>
        /// The related entity type. This does not necessarily match the navigation property type.
        /// In the case of a HasMany relationship, this value will be the generic argument type.
        /// </summary>
        /// 
        /// <example>
        /// <code>
        /// public List&lt;Tag&gt; Tags { get; set; } // Type => Tag
        /// </code>
        /// </example>
        public Type RightType { get; internal set; }

        /// <summary>
        /// The parent entity type. This is the type of the class in which this attribute was used.
        /// </summary>
        public Type LeftType { get; internal set; }

        public bool IsHasMany => GetType() == typeof(HasManyAttribute) || GetType().Inherits(typeof(HasManyAttribute));
        public bool IsHasOne => GetType() == typeof(HasOneAttribute);

        /// <summary>
        /// Configures which links to show in the <see cref="RelationshipLinks"/>
        /// object for this relationship.
        /// </summary>
        public Link RelationshipLinks { get; }
        public bool CanInclude { get; }
        public string EntityPropertyName { get; }

        public abstract void SetValue(object entity, object newValue);

        public abstract object GetValue(object entity);

        public override string ToString()
        {
            return base.ToString() + ":" + PublicRelationshipName;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is RelationshipAttribute attr))
            {
                return false;
            }
            bool equalRelationshipName = PublicRelationshipName.Equals(attr.PublicRelationshipName);

            return IsHasMany == attr.IsHasMany && equalRelationshipName;
        }

        /// <summary>
        /// Whether or not the provided exposed name is equivalent to the one defined in on the model
        /// </summary>
        public virtual bool Is(string publicRelationshipName)
            => string.Equals(publicRelationshipName, PublicRelationshipName, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// The internal navigation property path to the related entity.
        /// </summary>
        /// <remarks>
        /// In all cases except the HasManyThrough relationships, this will just be the <see cref="InternalRelationshipName" />.
        /// </remarks>
        public virtual string RelationshipPath => InternalRelationshipName;
    }
}
