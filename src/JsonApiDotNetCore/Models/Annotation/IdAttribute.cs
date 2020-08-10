using System;

namespace JsonApiDotNetCore.Models.Annotation
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class IdAttribute : Attribute
    {
        /// <summary>
        /// Identifies the field that will be used as the json api id field
        /// </summary>
        /// <example>
        /// <code><![CDATA[
        /// public class Author : IIdentifiable<string>
        /// {
        ///     [Id]
        ///     public string AuthorId { get; set; }
        /// }
        /// ]]></code>
        /// </example>
        public IdAttribute()
        {
        }

    }
}
