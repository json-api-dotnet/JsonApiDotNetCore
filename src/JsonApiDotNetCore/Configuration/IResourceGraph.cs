using System.Linq.Expressions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Configuration;

/// <summary>
/// Metadata about the shape of JSON:API resources that your API serves and the relationships between them. The resource graph is built at application
/// startup and is exposed as a singleton through Dependency Injection.
/// </summary>
[PublicAPI]
public interface IResourceGraph
{
    /// <summary>
    /// Gets the metadata for all registered resources.
    /// </summary>
    IReadOnlySet<ResourceType> GetResourceTypes();

    /// <summary>
    /// Gets the metadata for the resource that is publicly exposed by the specified name. Throws an <see cref="InvalidOperationException" /> when not found.
    /// </summary>
    ResourceType GetResourceType(string publicName);

    /// <summary>
    /// Gets the metadata for the resource of the specified CLR type. Throws an <see cref="InvalidOperationException" /> when not found.
    /// </summary>
    ResourceType GetResourceType(Type resourceClrType);

    /// <summary>
    /// Gets the metadata for the resource of the specified CLR type. Throws an <see cref="InvalidOperationException" /> when not found.
    /// </summary>
    ResourceType GetResourceType<TResource>()
        where TResource : class, IIdentifiable;

    /// <summary>
    /// Attempts to get the metadata for the resource that is publicly exposed by the specified name. Returns <c>null</c> when not found.
    /// </summary>
    ResourceType? FindResourceType(string publicName);

    /// <summary>
    /// Attempts to get metadata for the resource of the specified CLR type. Returns <c>null</c> when not found.
    /// </summary>
    ResourceType? FindResourceType(Type resourceClrType);

    /// <summary>
    /// Attempts to get metadata for the resource of the specified CLR type. Returns <c>null</c> when not found.
    /// </summary>
    ResourceType? FindResourceType<TResource>()
        where TResource : class, IIdentifiable;

    /// <summary>
    /// Gets the metadata for all registered compound types.
    /// </summary>
    IReadOnlySet<CompoundType> GetCompoundTypes();

    /// <summary>
    /// Gets the metadata for the compound type of the specified CLR type. Throws an <see cref="InvalidOperationException" /> when not found.
    /// </summary>
    CompoundType GetCompoundType(Type clrType);

    /// <summary>
    /// Gets the metadata for the compound type of the specified CLR type. Throws an <see cref="InvalidOperationException" /> when not found.
    /// </summary>
    CompoundType GetCompoundType<TCompound>();

    /// <summary>
    /// Attempts to get metadata for the compound type of the specified CLR type. Returns <c>null</c> when not found.
    /// </summary>
    CompoundType? FindCompoundType(Type clrType);

    /// <summary>
    /// Attempts to get metadata for the compound type of the specified CLR type. Returns <c>null</c> when not found.
    /// </summary>
    CompoundType? FindCompoundType<TCompound>();

    /// <summary>
    /// Gets the fields (attributes and relationships) for <typeparamref name="TResource" /> that are targeted by the selector.
    /// </summary>
    /// <typeparam name="TResource">
    /// The resource CLR type for which to retrieve fields.
    /// </typeparam>
    /// <param name="selector">
    /// Should be of the form: <![CDATA[
    /// (TResource resource) => new { resource.Attribute1, resource.Relationship2 }
    /// ]]>
    /// </param>
    IReadOnlyCollection<ResourceFieldAttribute> GetFields<TResource>(Expression<Func<TResource, object?>> selector)
        where TResource : class, IIdentifiable;

    /// <summary>
    /// Gets the attributes for <typeparamref name="TResource" /> that are targeted by the selector.
    /// </summary>
    /// <typeparam name="TResource">
    /// The resource CLR type for which to retrieve attributes.
    /// </typeparam>
    /// <param name="selector">
    /// Should be of the form: <![CDATA[
    /// (TResource resource) => new { resource.Attribute1, resource.Attribute2 }
    /// ]]>
    /// </param>
    IReadOnlyCollection<AttrAttribute> GetAttributes<TResource>(Expression<Func<TResource, object?>> selector)
        where TResource : class, IIdentifiable;

    /// <summary>
    /// Gets the relationships for <typeparamref name="TResource" /> that are targeted by the selector.
    /// </summary>
    /// <typeparam name="TResource">
    /// The resource CLR type for which to retrieve relationships.
    /// </typeparam>
    /// <param name="selector">
    /// Should be of the form: <![CDATA[
    /// (TResource resource) => new { resource.Relationship1, resource.Relationship2 }
    /// ]]>
    /// </param>
    IReadOnlyCollection<RelationshipAttribute> GetRelationships<TResource>(Expression<Func<TResource, object?>> selector)
        where TResource : class, IIdentifiable;

    /// <summary>
    /// Resolves the specified property chain selector into a <see cref="ResourceFieldChainExpression" />.
    /// </summary>
    /// <typeparam name="TResource">
    /// The resource CLR type for which to resolve the field chain.
    /// </typeparam>
    /// <param name="selector">
    /// Should be of the form: <![CDATA[
    /// (TResource resource) => resource.Field1.ChildField2
    /// ]]>
    /// </param>
    ResourceFieldChainExpression GetFieldChain<TResource>(Expression<Func<TResource, object?>> selector)
        where TResource : class, IIdentifiable;

    /// <summary>
    /// Resolves the specified property chain selector into a collection of <see cref="ResourceFieldChainExpression" />s.
    /// </summary>
    /// <typeparam name="TResource">
    /// The resource CLR type for which to resolve field chains.
    /// </typeparam>
    /// <param name="selector">
    /// Should be of the form: <![CDATA[
    /// (TResource resource) => resource.Field1.ChildField2
    /// ]]>
    /// or: <![CDATA[
    /// (TResource resource) => new { resource.Field1, resource.Field2.ChildField3 }
    /// ]]>
    /// </param>
    IReadOnlyCollection<ResourceFieldChainExpression> GetFieldChains<TResource>(Expression<Func<TResource, object?>> selector)
        where TResource : class, IIdentifiable;
}
