using System;

namespace JsonApiDotNetCore.Resources.Annotations
{
    /// <summary>
    /// Indicates capabilities that can be performed on an <see cref="AttrAttribute" />.
    /// </summary>
    [Flags]
    public enum AttrCapabilities
    {
        None = 0,

        /// <summary>
        /// Whether or not GET requests can retrieve the attribute. Attempts to retrieve when disabled will return an HTTP 400 response.
        /// </summary>
        AllowView = 1,

        /// <summary>
        /// Whether or not POST requests can assign the attribute value. Attempts to assign when disabled will return an HTTP 422 response.
        /// </summary>
        AllowCreate = 2,

        /// <summary>
        /// Whether or not PATCH requests can update the attribute value. Attempts to update when disabled will return an HTTP 422 response.
        /// </summary>
        AllowChange = 4,

        /// <summary>
        /// Whether or not an attribute can be filtered on via a query string parameter. Attempts to filter when disabled will return an HTTP 400 response.
        /// </summary>
        AllowFilter = 8,

        /// <summary>
        /// Whether or not an attribute can be sorted on via a query string parameter. Attempts to sort when disabled will return an HTTP 400 response.
        /// </summary>
        AllowSort = 16,

        All = AllowView | AllowCreate | AllowChange | AllowFilter | AllowSort
    }
}
