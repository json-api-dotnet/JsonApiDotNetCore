using System;
using System.Collections;
using System.Collections.Generic;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Hooks
{
    /// <summary>
    /// A class used internally for resource hook execution. Not intended for developer use.
    /// 
    /// A wrapper for RelationshipAttribute with an abstraction layer that works on the 
    /// getters and setters of relationships. These are different in the case of 
    /// HasMany vs HasManyThrough, and HasManyThrough.
    /// It also depends on if the join table entity
    /// (eg ArticleTags) is identifiable (in which case we will traverse through 
    /// it and fire hooks for it, if defined) or not (in which case we skip 
    /// ArticleTags and go directly to Tags.
    /// </summary>
    internal sealed class RelationshipProxy
    {
        private readonly bool _isHasManyThrough;
        private readonly bool _skipJoinTable;

        /// <summary>
        /// The target type for this relationship attribute. 
        /// For HasOne has HasMany this is trivial: just the right-hand side.
        /// For HasManyThrough it is either the ThroughProperty (when the join table is 
        /// Identifiable) or it is the right-hand side (when the join table is not identifiable)
        /// </summary>
        public Type RightType { get; }
        public Type LeftType => Attribute.LeftType;
        public bool IsContextRelation { get; }

        public RelationshipAttribute Attribute { get; set; }
        public RelationshipProxy(RelationshipAttribute attr, Type relatedType, bool isContextRelation)
        {
            RightType = relatedType;
            Attribute = attr;
            IsContextRelation = isContextRelation;
            if (attr is HasManyThroughAttribute throughAttr)
            {
                _isHasManyThrough = true;
                _skipJoinTable |= RightType != throughAttr.ThroughType;
            }
        }

        /// <summary>
        /// Gets the relationship value for a given parent entity.
        /// Internally knows how to do this depending on the type of RelationshipAttribute
        /// that this RelationshipProxy encapsulates.
        /// </summary>
        /// <returns>The relationship value.</returns>
        /// <param name="entity">Parent entity.</param>
        public object GetValue(IIdentifiable entity)
        {
            if (_isHasManyThrough)
            {
                var throughAttr = (HasManyThroughAttribute)Attribute;
                if (!_skipJoinTable)
                {
                    return throughAttr.ThroughProperty.GetValue(entity);
                }
                var collection = new List<IIdentifiable>();
                var joinEntities = (IEnumerable)throughAttr.ThroughProperty.GetValue(entity);
                if (joinEntities == null) return null;

                foreach (var joinEntity in joinEntities)
                {
                    var rightEntity = (IIdentifiable)throughAttr.RightProperty.GetValue(joinEntity);
                    if (rightEntity == null) continue;
                    collection.Add(rightEntity);
                }
                return collection;
            }
            return Attribute.GetValue(entity);
        }

        /// <summary>
        /// Set the relationship value for a given parent entity.
        /// Internally knows how to do this depending on the type of RelationshipAttribute
        /// that this RelationshipProxy encapsulates.
        /// </summary>
        /// <param name="entity">Parent entity.</param>
        /// <param name="value">The relationship value.</param>
        public void SetValue(IIdentifiable entity, object value)
        {
            if (_isHasManyThrough)
            {
                if (!_skipJoinTable)
                {
                    var list = (IEnumerable<object>)value;
                    ((HasManyThroughAttribute)Attribute).ThroughProperty.SetValue(entity, list.CopyToList(RightType));
                    return;
                }
                var throughAttr = (HasManyThroughAttribute)Attribute;
                var joinEntities = (IEnumerable<object>)throughAttr.ThroughProperty.GetValue(entity);

                var filteredList = new List<object>();
                var rightEntities = ((IEnumerable<object>)value).CopyToList(RightType);
                foreach (var je in joinEntities)
                {

                    if (((IList)rightEntities).Contains(throughAttr.RightProperty.GetValue(je)))
                    {
                        filteredList.Add(je);
                    }
                }
                throughAttr.ThroughProperty.SetValue(entity, filteredList.CopyToList(throughAttr.ThroughType));
                return;
            }
            Attribute.SetValue(entity, value);
        }
    }
}
