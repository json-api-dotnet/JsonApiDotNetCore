using System;
using System.Runtime.CompilerServices;
using JsonApiDotNetCore.Extensions;

namespace JsonApiDotNetCore.Models
{
    public abstract class RelationshipAttribute : Attribute
    {
        protected RelationshipAttribute(string publicName, Link documentLinks, bool canInclude, string mappedBy, string inverseNavigationProperty = null)
        {
            PublicRelationshipName = publicName;
            DocumentLinks = documentLinks;
            CanInclude = canInclude;
            EntityPropertyName = mappedBy;
            InverseNavigation = inverseNavigationProperty;
        }

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
        [Obsolete("Use property DependentType")]
        public Type Type { get { return DependentType; } internal set { DependentType = value; } }

        /// <summary>
        /// The related entity type. This does not necessarily match the navigation property type.
        /// In the case of a HasMany relationship, this value will be the generic argument type.
        /// 
        /// The technical language as used in EF Core is used here (dependent vs principal).
        /// </summary>
        /// 
        /// <example>
        /// <code>
        /// public List&lt;Tag&gt; Tags { get; set; } // Type => Tag
        /// </code>
        /// </example>
        public Type DependentType { get; internal set; }

        /// <summary>
        /// The parent entity type. The technical language as used in EF Core is used here (dependent vs principal).
        /// </summary>
        public Type PrincipalType { get; internal set; }

        public bool IsHasMany => GetType() == typeof(HasManyAttribute) || GetType().Inherits(typeof(HasManyAttribute));
        public bool IsHasOne => GetType() == typeof(HasOneAttribute);
        public Link DocumentLinks { get; } = Link.All;
        public bool CanInclude { get; }
        public string EntityPropertyName { get; }

        public string InverseNavigation { get ; internal set;}

        public bool TryGetHasOne(out HasOneAttribute result)
        {
            if (IsHasOne)
            {
                result = (HasOneAttribute)this;
                return true;
            }
            result = null;
            return false;
        }

        public bool TryGetHasMany(out HasManyAttribute result)
        {
            if (IsHasMany)
            {
                result = (HasManyAttribute)this;
                return true;
            }
            result = null;
            return false;
        }

        public abstract void SetValue(object entity, object newValue);
        
        public object GetValue(object entity) => entity
            ?.GetType()?
            .GetProperty(InternalRelationshipName)?
            .GetValue(entity);

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

            bool equalPrincipalType = true;
            if (PrincipalType != null)
            {
                equalPrincipalType = PrincipalType.Equals(attr.PrincipalType);
            }
            return IsHasMany == attr.IsHasMany && equalRelationshipName && equalPrincipalType;
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
