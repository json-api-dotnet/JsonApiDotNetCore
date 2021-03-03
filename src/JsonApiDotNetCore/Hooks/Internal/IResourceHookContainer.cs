using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Hooks.Internal
{
    /// <summary>
    /// Not meant for public usage. Used internally in the <see cref="ResourceHookExecutor" />
    /// </summary>
    public interface IResourceHookContainer
    {
    }

    /// <summary>
    /// Implement this interface to implement business logic hooks on <see cref="ResourceHooksDefinition{TResource}" />.
    /// </summary>
    public interface IResourceHookContainer<TResource>
        : IReadHookContainer<TResource>, IDeleteHookContainer<TResource>, ICreateHookContainer<TResource>, IUpdateHookContainer<TResource>,
            IOnReturnHookContainer<TResource>, IResourceHookContainer
        where TResource : class, IIdentifiable
    {
    }
}
