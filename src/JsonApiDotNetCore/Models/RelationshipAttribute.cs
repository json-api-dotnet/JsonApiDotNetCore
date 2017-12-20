using System;

namespace JsonApiDotNetCore.Models
{
    public abstract class RelationshipAttribute : Attribute
    {
        protected RelationshipAttribute(string publicName, Link documentLinks)
        {
            PublicRelationshipName = publicName;
            DocumentLinks = documentLinks;
        }

        public string PublicRelationshipName { get; }
        public string InternalRelationshipName { get; internal set; }
        public Type Type { get; internal set; }
        public bool IsHasMany => GetType() == typeof(HasManyAttribute);
        public bool IsHasOne => GetType() == typeof(HasOneAttribute);
        public Link DocumentLinks { get; } = Link.All;

        public abstract void SetValue(object entity, object newValue);

        public override string ToString()
        {
            return base.ToString() + ":" + PublicRelationshipName;
        }

        public override bool Equals(object obj)
        {
            var attr = obj as RelationshipAttribute;
            if (attr == null)
            {
                return false;
            }
            return IsHasMany == attr.IsHasMany && PublicRelationshipName.Equals(attr.PublicRelationshipName);
        }
    }
}
