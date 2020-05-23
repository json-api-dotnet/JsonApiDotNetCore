using JsonApiDotNetCore.Models.Links;
using JsonApiDotNetCore.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace JsonApiDotNetCore.Models.Fluent
{
    /// <summary>
    /// Provides a developer with fluent api for mapping their resources. 
    /// It is intended as an alternative for annotations. 
    /// When both methods are used, fluent mapping overrides annotations. 
    /// </summary>
    /// <typeparam name="TResource">The resource type</typeparam>
    public abstract class ResourceMapping<TResource>: IResourceMapping
    where TResource : class
    {
        private ResourceAttribute _resource;
        private LinksAttribute _links;
        private List<AttrAttribute> _attributes;
        private List<RelationshipAttribute> _relationships;
        private List<EagerLoadAttribute> _eagerLoads;

        public ResourceMapping()
        {
            _attributes = new List<AttrAttribute>();
            _relationships = new List<RelationshipAttribute>();
            _eagerLoads = new List<EagerLoadAttribute>();
        }

        public ResourceAttribute Resource
        {
            get
            {
                return _resource;
            }
        }

        public LinksAttribute Links
        {
            get
            {
                return _links;
            }
        }

        public List<AttrAttribute> Attributes
        {
            get
            {
                return _attributes;
            }
        }

        public List<RelationshipAttribute> Relationships
        {
            get
            {
                return _relationships;
            }
        }

        public List<EagerLoadAttribute> EagerLoads
        {
            get
            {
                return _eagerLoads;
            }
        }

        /// <summary>
        /// Resource name.
        /// </summary>
        public void ResourceName(string resourceName)
        {
            _resource = CreateResourceAttribute(resourceName);
        }

        public TopLevelLinks TopLevelLinks()
        {
            if (_links is null)
            {
                _links = CreateLinkAttribute();
            }

            return new TopLevelLinks(_links);
        }

        public ResourceLinks ResourceLinks()
        {
            if (_links is null)
            {
                _links = CreateLinkAttribute();
            }

            return new ResourceLinks(_links);
        }

        public RelationshipLinks RelationshipLinks()
        {
            if (_links is null)
            {
                _links = CreateLinkAttribute();
            }

            return new RelationshipLinks(_links);
        }

        /// <summary>
        /// Exposes a resource property as a json:api attribute with an explicit name, using configured capabilities.
        /// </summary>           
        public Property Property(Expression<Func<TResource, object>> memberExpression)
        {
            AttrAttribute attribute = CreateAttrAttribute(memberExpression);

            _attributes.Add(attribute);

            return new Property(attribute);
        }

        /// <summary>
        /// Create a HasOne relational link to another entity.
        /// </summary>     
        public HasOne HasOne(Expression<Func<TResource, object>> memberExpression)
        {
            HasOneAttribute attribute = CreateHasOneAttribute(memberExpression);

            var existingAttribute = _relationships.Where(x => x.PropertyInfo.Equals(attribute.PropertyInfo))
                                                  .FirstOrDefault() as HasOneAttribute;

            if (existingAttribute is null)
            {
                _relationships.Add(attribute);
            }
            else
            {
                attribute = existingAttribute;
            }

            return new HasOne(attribute);
        }

        /// <summary>
        /// Create a HasMany relational link to another entity
        /// </summary>
        public HasMany HasMany(Expression<Func<TResource, object>> memberExpression)
        {
            HasManyAttribute attribute = CreateHasManyAttribute(memberExpression);

            var existingAttribute = _relationships.Where(x => x.PropertyInfo.Equals(attribute.PropertyInfo))
                                                  .FirstOrDefault() as HasManyAttribute;

            if (existingAttribute is null)
            {
                _relationships.Add(attribute);
            }
            else
            {
                attribute = existingAttribute;
            }

            return new HasMany(attribute);
        }

        /// <summary>
        /// Create a HasMany relationship through a many-to-many join relationship.
        /// This type can only be applied on types that implement ICollection.
        /// </summary>
        public HasManyThrough HasManyThrough(Expression<Func<TResource, object>> memberExpression, Expression<Func<TResource, object>> throughExpression)
        {
            HasManyThroughAttribute attribute = CreateHasManyThroughAttribute(memberExpression, throughExpression);

            var existingAttribute = _relationships.Where(x => x.PropertyInfo.Equals(attribute.PropertyInfo))
                                                  .FirstOrDefault() as HasManyThroughAttribute;

            if (existingAttribute is null)
            {
                _relationships.Add(attribute);
            }
            else
            {
                attribute = existingAttribute;
            }

            return new HasManyThrough(attribute);
        }

        /// <summary>
        /// Used to unconditionally load a related entity that is not exposed as a json:api relationship.
        /// </summary>
        /// <remarks>
        /// This is intended for calculated properties that are exposed as json:api attributes, which depend on a related entity to always be loaded.        
        /// </remarks>
        public EagerLoad EagerLoad(Expression<Func<TResource, object>> memberExpression)
        {
            EagerLoadAttribute attribute = CreateEagerLoadAttribute(memberExpression);

            _eagerLoads.Add(attribute);

            return new EagerLoad(attribute);
        }

        private static ResourceAttribute CreateResourceAttribute(string resourceName)
        {
            ResourceAttribute attribute = new ResourceAttribute(resourceName);

            return attribute;
        }

        private static LinksAttribute CreateLinkAttribute()
        {
            LinksAttribute attribute = new LinksAttribute();

            return attribute;
        }

        private static AttrAttribute CreateAttrAttribute(Expression<Func<TResource, object>> memberExpression)
        {            
            Member member = memberExpression.ToMember();

            AttrAttribute attribute = new AttrAttribute();

            attribute.PropertyInfo = member.DeclaringType.GetProperty(member.Name);

            return attribute;
        }

        private static HasOneAttribute CreateHasOneAttribute(Expression<Func<TResource, object>> memberExpression)
        {
            Member member = memberExpression.ToMember();

            HasOneAttribute attribute = new HasOneAttribute();

            attribute.PropertyInfo = member.DeclaringType.GetProperty(member.Name);

            return attribute;
        }

        private static HasManyAttribute CreateHasManyAttribute(Expression<Func<TResource, object>> memberExpression)
        {
            Member member = memberExpression.ToMember();

            HasManyAttribute attribute = new HasManyAttribute();

            attribute.PropertyInfo = member.DeclaringType.GetProperty(member.Name);

            return attribute;
        }

        private static HasManyThroughAttribute CreateHasManyThroughAttribute(Expression<Func<TResource, object>> memberExpression, Expression<Func<TResource, object>> throughExpression)
        {
            Member member = memberExpression.ToMember();
            Member through = throughExpression.ToMember();

            HasManyThroughAttribute attribute = new HasManyThroughAttribute(through.Name);

            attribute.PropertyInfo = member.DeclaringType.GetProperty(member.Name);
            attribute.ThroughProperty = member.DeclaringType.GetProperty(through.Name);

            return attribute;
        }

        private static EagerLoadAttribute CreateEagerLoadAttribute(Expression<Func<TResource, object>> memberExpression)
        {
            Member member = memberExpression.ToMember();

            EagerLoadAttribute attribute = new EagerLoadAttribute();

            attribute.Property = member.DeclaringType.GetProperty(member.Name);

            return attribute;
        }
    }
}
