using System.Collections.Generic;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Serialization.Building
{
    /// <summary>
    /// Builds the top-level meta data object. This builder is generic to allow for
    /// different top-level meta data objects depending on the associated resource of the request.
    /// </summary>
    /// <typeparam name="TResource">Associated resource for which to build the meta data</typeparam>
    public interface IMetaBuilder<TResource> where TResource : class, IIdentifiable
    {
        /// <summary>
        /// Adds a key-value pair to the top-level meta data object
        /// </summary>
        void Add(string key, object value);
        /// <summary>
        /// Joins the new dictionary with the current one. In the event of a key collision,
        /// the new value will override the old.
        /// </summary>
        void Add(IReadOnlyDictionary<string,object> values);
        /// <summary>
        /// Builds the top-level meta data object.
        /// </summary>
        IDictionary<string, object> GetMeta();
    }
}
