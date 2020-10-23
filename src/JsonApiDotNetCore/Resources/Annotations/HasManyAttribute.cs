using System;
using System.Collections.Generic;

namespace JsonApiDotNetCore.Resources.Annotations
{
    /// <summary>
    /// Used to expose a property on a resource class as a json:api to-many relationship (https://jsonapi.org/format/#document-resource-object-relationships).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class HasManyAttribute : RelationshipAttribute
    {
        /// <summary>
        /// Creates a HasMany relational link to another resource.
        /// </summary>
        /// <example>
        /// <code><![CDATA[
        /// public class Author : Identifiable 
        /// {
        ///     [HasMany(PublicName = "articles")]
        ///     public List<Article> Articles { get; set; }
        /// }
        /// ]]></code>
        /// </example>
        public HasManyAttribute()
        {
            Links = LinkTypes.All;
        }
        
        internal virtual IEnumerable<IIdentifiable> GetManyValue(object resource, IResourceFactory resourceFactory = null)
        {
            if (resource == null) throw new ArgumentNullException(nameof(resource));
            
            return (IEnumerable<IIdentifiable>)base.GetValue(resource);
        }
    }
}
