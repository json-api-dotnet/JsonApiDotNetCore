using System;
using System.Reflection;

namespace JsonApiDotNetCore.Models
{
    /// <summary>
    /// 
    /// </summary>
    /// 
    /// <example>
    /// 
    /// </example>
    public class HasManyThroughAttribute : HasManyAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <example>
        /// 
        /// </example>
        public HasManyThroughAttribute(string publicName, string internalThroughName, Link documentLinks = Link.All, bool canInclude = true)
        : base(publicName, documentLinks, canInclude)
        {
            InternalThroughName = internalThroughName;
        }

        public string InternalThroughName { get; private set; }
        public Type ThroughType { get; internal set; }

        /// <summary>
        /// The navigation property back to the parent resource.
        /// </summary>
        /// 
        /// <example>
        /// In the `[HasManyThrough("tags", nameof(ArticleTags))]` example
        /// this would point to the `Article.ArticleTags.Article` property
        ///
        /// <code>
        /// public Article Article { get; set; }
        /// </code>
        ///
        /// </example>
        public PropertyInfo LeftProperty { get; internal set; }

        /// <summary>
        /// The navigation property to the related resource.
        /// </summary>
        /// 
        /// <example>
        /// In the `[HasManyThrough("tags", nameof(ArticleTags))]` example
        /// this would point to the `Article.ArticleTags.Tag` property
        ///
        /// <code>
        /// public Tag Tag { get; set; }
        /// </code>
        ///
        /// </example>
        public PropertyInfo RightProperty { get; internal set; }
    }
}