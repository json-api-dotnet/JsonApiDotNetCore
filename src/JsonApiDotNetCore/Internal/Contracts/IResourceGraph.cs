using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Internal.Contracts
{
    /// <summary>
    /// A cache for the models in entity core
    /// </summary>
    public interface IResourceGraph : IContextEntityProvider
    {
        /// <summary>
        /// Was built against an EntityFrameworkCore DbContext ?
        /// </summary>
        bool UsesDbContext { get; }
    }
}
