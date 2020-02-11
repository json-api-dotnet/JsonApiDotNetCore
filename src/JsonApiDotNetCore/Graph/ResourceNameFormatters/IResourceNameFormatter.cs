using System;
using System.Reflection;

namespace JsonApiDotNetCore.Graph
{
    /// <summary>
    /// Provides an interface for formatting resource names by convention
    /// </summary>
    public interface IResourceNameFormatter
    {
        /// <summary>
        /// Get the publicly visible resource name from the internal type name
        /// </summary>
        string FormatResourceName(Type resourceType);

        /// <summary>
        /// Get the publicly visible name for the given property
        /// </summary>
        string FormatPropertyName(PropertyInfo property);

        /// <summary>
        /// Applies the desired casing convention to the internal string.
        /// This is generally applied to the type name after pluralization.
        /// </summary>
        string ApplyCasingConvention(string properName);
    }
}
