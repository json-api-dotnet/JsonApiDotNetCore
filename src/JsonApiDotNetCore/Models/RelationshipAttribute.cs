using System;

namespace JsonApiDotNetCore.Models
{
    public abstract class RelationshipAttribute : Attribute
    {
        protected RelationshipAttribute(string publicName, Link documentLinks, bool canInclude)
        {
            PublicRelationshipName = publicName;
            DocumentLinks = documentLinks;
            CanInclude = canInclude;
        }

        public string PublicRelationshipName { get; }
        public string InternalRelationshipName { get; internal set; }
        public Type Type { get; internal set; }
        public bool IsHasMany => GetType() == typeof(HasManyAttribute);
        public bool IsHasOne => GetType() == typeof(HasOneAttribute);
        public Link DocumentLinks { get; } = Link.All;
        public bool CanInclude { get; }

        public abstract void SetValue(object entity, object newValue);

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
    }
}
