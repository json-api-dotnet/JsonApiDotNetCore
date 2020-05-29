using System;
using JsonApiDotNetCore.Models.Links;

namespace JsonApiDotNetCore.Models.Annotation
{
    [AttributeUsage(AttributeTargets.Property)]
    public class HasManyAttribute : RelationshipAttribute
    {
        /// <summary>
        /// Create a HasMany relational link to another entity
        /// </summary>
        /// 
        /// <param name="publicName">The relationship name as exposed by the API</param>
        /// <param name="relationshipLinks">Which links are available. Defaults to <see cref="Link.All"/></param>
        /// <param name="canInclude">Whether or not this relationship can be included using the <c>?include=public-name</c> query string</param>
        /// <param name="inverseNavigationProperty"></param>
        /// <example>
        /// <code><![CDATA[
        /// public class Author : Identifiable 
        /// {
        ///     [HasMany("articles"]
        ///     public List<Article> Articles { get; set; }
        /// }
        /// ]]></code>
        /// </example>
        public HasManyAttribute(string publicName = null, Link relationshipLinks = Link.All, bool canInclude = true, string inverseNavigationProperty = null)
        : base(publicName, relationshipLinks, canInclude)
        {
            InverseNavigation = inverseNavigationProperty;
        }
    }
}
