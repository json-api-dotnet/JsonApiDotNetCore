using System;
using JsonApiDotNetCore.Models.JsonApiDocuments;

namespace JsonApiDotNetCore.Models.Annotation
{
    [AttributeUsage(AttributeTargets.Property)]
    public class HasManyAttribute : RelationshipAttribute
    {
        /// <summary>
        /// Create a HasMany relational link to another resource
        /// </summary>
        /// 
        /// <param name="publicName">The relationship name as exposed by the API</param>
        /// <param name="relationshipLinks">Which links are available. Defaults to <see cref="Links.All"/></param>
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
        public HasManyAttribute(string publicName = null, Links relationshipLinks = Links.All, bool canInclude = true, string inverseNavigationProperty = null)
        : base(publicName, relationshipLinks, canInclude)
        {
            InverseNavigation = inverseNavigationProperty;
        }
    }
}
