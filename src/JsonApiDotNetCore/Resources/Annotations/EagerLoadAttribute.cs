using System;
using System.Collections.Generic;
using System.Reflection;

namespace JsonApiDotNetCore.Resources.Annotations
{
    /// <summary>
    /// Used to unconditionally load a related entity that is not exposed as a JSON:API relationship.
    /// </summary>
    /// <remarks>
    /// This is intended for calculated properties that are exposed as JSON:API attributes, which depend on a related entity to always be loaded.
    /// <example><![CDATA[
    /// public class User : Identifiable
    /// {
    ///     [Attr(AttrCapabilities.AllowFilter | AttrCapabilities.AllowSort)] 
    ///     [NotMapped]
    ///     public string DisplayName => Name.First + " " + Name.Last;
    /// 
    ///     [EagerLoad]
    ///     public Name Name { get; set; }
    /// }
    /// 
    /// public class Name // not exposed as resource, only database table
    /// {
    ///     public string First { get; set; }
    ///     public string Last { get; set; }
    /// }
    /// 
    /// public class Blog : Identifiable
    /// {
    ///     [HasOne]
    ///     public User Author { get; set; }
    /// }
    /// ]]></example>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class EagerLoadAttribute : Attribute
    {
        public PropertyInfo Property { get; internal set; }

        public IReadOnlyCollection<EagerLoadAttribute> Children { get; internal set; }
    }
}
