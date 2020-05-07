using System;
using System.Collections;
using System.Collections.Generic;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
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
            if (Attribute is HasManyThroughAttribute hasManyThrough)
            {
                if (!_skipJoinTable)
                {
                    return hasManyThrough.ThroughProperty.GetValue(entity);
                }
                var collection = new List<IIdentifiable>();
                var joinEntities = (IEnumerable)hasManyThrough.ThroughProperty.GetValue(entity);
                if (joinEntities == null) return null;

                foreach (var joinEntity in joinEntities)
                {
                    var rightEntity = (IIdentifiable)hasManyThrough.RightProperty.GetValue(joinEntity);
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
        /// <param name="resourceFactory"></param>
        public void SetValue(IIdentifiable entity, object value, IResourceFactory resourceFactory)
        {
            if (Attribute is HasManyThroughAttribute hasManyThrough)
            {
                if (!_skipJoinTable)
                {
                    hasManyThrough.ThroughProperty.SetValue(entity, value);
                    return;
                }

                var joinEntities = (IEnumerable)hasManyThrough.ThroughProperty.GetValue(entity);

                var filteredList = new List<object>();
                var rightEntities = ((IEnumerable)value).CopyToList(RightType);
                foreach (var joinEntity in joinEntities)
                {
                    if (((IList)rightEntities).Contains(hasManyThrough.RightProperty.GetValue(joinEntity)))
                    {
                        filteredList.Add(joinEntity);
                    }
                }

                var collectionValue = filteredList.CopyToTypedCollection(hasManyThrough.ThroughProperty.PropertyType);
                hasManyThrough.ThroughProperty.SetValue(entity, collectionValue);
                return;
            }

            Attribute.SetValue(entity, value, resourceFactory);
        }
    }
}
