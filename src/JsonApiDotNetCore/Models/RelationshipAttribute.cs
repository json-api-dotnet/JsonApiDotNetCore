using System;
using System.Reflection;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Models
{
    public abstract class RelationshipAttribute : Attribute
    {
        public string PublicName
        {
            get => PublicRelationshipName;
            set => PublicRelationshipName = value;
        }

        private LinkTypes _links;

        /// <summary>
        /// Configures which links to show in the <see cref="Serialization.Objects.RelationshipLinks" /> object for this relationship. Defaults to
        /// <see cref="LinkTypes.NotConfigured" />, which falls back to <see cref="ResourceLinksAttribute.RelationshipLinks" /> and then falls back to
        /// <see cref="IJsonApiOptions.RelationshipLinks" />.
        /// </summary>
        public LinkTypes Links
        {
            get => DocumentLinks;
            set => DocumentLinks = value;
        }

        public RelationshipAttribute()
        {
            Links = LinkTypes.All;
            CanInclude = true;
        }

        // protected RelationshipAttribute(string publicName, LinkTypes documentLinks, bool canInclude, string mappedBy)
        // {
        //     PublicRelationshipName = publicName;
        //     DocumentLinks = documentLinks;
        //     CanInclude = canInclude;
        //     EntityPropertyName = mappedBy;
        // }

        public string PublicRelationshipName { get; internal set; }
        public string InternalRelationshipName { get; internal set; }

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
        public Type Type { get; internal set; }

        public bool IsHasMany => GetType() == typeof(HasManyAttribute) || GetType().
            Inherits(typeof(HasManyAttribute));

        public bool IsHasOne => GetType() == typeof(HasOneAttribute);
        public LinkTypes DocumentLinks { get; private set; } = LinkTypes.All;
        public bool CanInclude { get; }
        public string EntityPropertyName { get; }

        public bool TryGetHasOne(out HasOneAttribute result)
        {
            if (IsHasOne)
            {
                result = (HasOneAttribute) this;
                return true;
            }

            result = null;
            return false;
        }

        public bool TryGetHasMany(out HasManyAttribute result)
        {
            if (IsHasMany)
            {
                result = (HasManyAttribute) this;
                return true;
            }

            result = null;
            return false;
        }

        public abstract void SetValue(object entity, object newValue);

        public object GetValue(object entity) => entity?.GetType()?
            .
            GetProperty(InternalRelationshipName)?
            .
            GetValue(entity);

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

            return IsHasMany == attr.IsHasMany && PublicRelationshipName.Equals(attr.PublicRelationshipName);
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
