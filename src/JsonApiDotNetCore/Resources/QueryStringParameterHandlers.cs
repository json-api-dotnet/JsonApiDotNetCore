using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCore.Resources;

/// <summary>
/// This is an alias type intended to simplify the implementation's method signature. See
/// <see cref="IResourceDefinition{TResource, TId}.OnRegisterQueryableHandlersForQueryStringParameters" /> for usage details.
/// </summary>
/// <typeparam name="TResource">
/// The resource type.
/// </typeparam>
public sealed class QueryStringParameterHandlers<TResource> : Dictionary<string, Func<IQueryable<TResource>, StringValues, IQueryable<TResource>>>;
