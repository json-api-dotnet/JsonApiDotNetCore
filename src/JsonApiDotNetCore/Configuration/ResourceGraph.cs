using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Configuration;

/// <inheritdoc cref="IResourceGraph" />
[PublicAPI]
public sealed class ResourceGraph : IResourceGraph
{
    private static readonly Type? ProxyTargetAccessorType = Type.GetType("Castle.DynamicProxy.IProxyTargetAccessor, Castle.Core");

    private static readonly IReadOnlySet<CompoundType> EmptyCompoundTypeSet = new HashSet<CompoundType>().AsReadOnly();

    private readonly IReadOnlySet<ResourceType> _resourceTypeSet;
    private readonly Dictionary<Type, ResourceType> _resourceTypesByClrType = [];
    private readonly Dictionary<string, ResourceType> _resourceTypesByPublicName = [];
    private readonly IReadOnlySet<CompoundType> _compoundTypeSet;
    private readonly Dictionary<Type, CompoundType> _compoundTypesByClrType = [];

    public ResourceGraph(IReadOnlySet<ResourceType> resourceTypeSet)
        : this(resourceTypeSet, EmptyCompoundTypeSet)
    {
    }

    public ResourceGraph(IReadOnlySet<ResourceType> resourceTypeSet, IReadOnlySet<CompoundType> compoundTypeSet)
    {
        ArgumentNullException.ThrowIfNull(resourceTypeSet);
        ArgumentNullException.ThrowIfNull(compoundTypeSet);

        _resourceTypeSet = resourceTypeSet;
        _compoundTypeSet = compoundTypeSet;

        foreach (ResourceType resourceType in resourceTypeSet)
        {
            _resourceTypesByClrType.Add(resourceType.ClrType, resourceType);
            _resourceTypesByPublicName.Add(resourceType.PublicName, resourceType);
        }

        foreach (CompoundType compoundType in compoundTypeSet)
        {
            _compoundTypesByClrType.Add(compoundType.ClrType, compoundType);
        }
    }

    /// <inheritdoc />
    public IReadOnlySet<ResourceType> GetResourceTypes()
    {
        return _resourceTypeSet;
    }

    /// <inheritdoc />
    public ResourceType GetResourceType(string publicName)
    {
        ResourceType? resourceType = FindResourceType(publicName);

        if (resourceType == null)
        {
            throw new InvalidOperationException($"Resource type '{publicName}' does not exist in the resource graph.");
        }

        return resourceType;
    }

    /// <inheritdoc />
    public ResourceType? FindResourceType(string publicName)
    {
        ArgumentNullException.ThrowIfNull(publicName);

        return _resourceTypesByPublicName.GetValueOrDefault(publicName);
    }

    /// <inheritdoc />
    public ResourceType GetResourceType(Type resourceClrType)
    {
        ResourceType? resourceType = FindResourceType(resourceClrType);

        if (resourceType == null)
        {
            throw new InvalidOperationException($"Type '{resourceClrType}' does not exist in the resource graph.");
        }

        return resourceType;
    }

    /// <inheritdoc />
    public ResourceType? FindResourceType(Type resourceClrType)
    {
        ArgumentNullException.ThrowIfNull(resourceClrType);

        Type typeToFind = IsLazyLoadingProxyForResourceType(resourceClrType) ? resourceClrType.BaseType! : resourceClrType;
        return _resourceTypesByClrType.GetValueOrDefault(typeToFind);
    }

    /// <inheritdoc />
    public IReadOnlySet<CompoundType> GetCompoundTypes()
    {
        return _compoundTypeSet;
    }

    /// <inheritdoc />
    public CompoundType GetCompoundType(Type clrType)
    {
        CompoundType? compoundType = FindCompoundType(clrType);

        if (compoundType == null)
        {
            throw new InvalidOperationException($"Compound type '{clrType}' does not exist in the resource graph.");
        }

        return compoundType;
    }

    /// <inheritdoc />
    public CompoundType GetCompoundType<TCompound>()
    {
        return GetCompoundType(typeof(TCompound));
    }

    /// <inheritdoc />
    public CompoundType? FindCompoundType(Type clrType)
    {
        ArgumentNullException.ThrowIfNull(clrType);

        return _compoundTypesByClrType.GetValueOrDefault(clrType);
    }

    /// <inheritdoc />
    public CompoundType? FindCompoundType<TCompound>()
    {
        return FindCompoundType(typeof(TCompound));
    }

    private bool IsLazyLoadingProxyForResourceType(Type resourceClrType)
    {
        return ProxyTargetAccessorType?.IsAssignableFrom(resourceClrType) ?? false;
    }

    /// <inheritdoc />
    public ResourceType GetResourceType<TResource>()
        where TResource : class, IIdentifiable
    {
        return GetResourceType(typeof(TResource));
    }

    /// <inheritdoc />
    public ResourceType? FindResourceType<TResource>()
        where TResource : class, IIdentifiable
    {
        return FindResourceType(typeof(TResource));
    }

    /// <inheritdoc />
    public IReadOnlyCollection<ResourceFieldAttribute> GetFields<TResource>(Expression<Func<TResource, object?>> selector)
        where TResource : class, IIdentifiable
    {
        ArgumentNullException.ThrowIfNull(selector);

        return FilterFields<TResource, ResourceFieldAttribute>(selector);
    }

    /// <inheritdoc />
    public IReadOnlyCollection<AttrAttribute> GetAttributes<TResource>(Expression<Func<TResource, object?>> selector)
        where TResource : class, IIdentifiable
    {
        ArgumentNullException.ThrowIfNull(selector);

        return FilterFields<TResource, AttrAttribute>(selector);
    }

    /// <inheritdoc />
    public IReadOnlyCollection<RelationshipAttribute> GetRelationships<TResource>(Expression<Func<TResource, object?>> selector)
        where TResource : class, IIdentifiable
    {
        ArgumentNullException.ThrowIfNull(selector);

        return FilterFields<TResource, RelationshipAttribute>(selector);
    }

    private ReadOnlyCollection<TField> FilterFields<TResource, TField>(Expression<Func<TResource, object?>> selector)
        where TResource : class, IIdentifiable
        where TField : ResourceFieldAttribute
    {
        IReadOnlyCollection<TField> source = GetFieldsOfType<TResource, TField>();
        List<TField> matches = [];

        foreach (string memberName in ToMemberNames(selector))
        {
            TField? matchingField = source.FirstOrDefault(field => field.Property.Name == memberName);

            if (matchingField == null)
            {
                throw new ArgumentException($"Member '{memberName}' is not exposed as a JSON:API field.", nameof(selector));
            }

            matches.Add(matchingField);
        }

        return matches.AsReadOnly();
    }

    private IReadOnlyCollection<TKind> GetFieldsOfType<TResource, TKind>()
        where TKind : ResourceFieldAttribute
    {
        ResourceType resourceType = GetResourceType(typeof(TResource));

        if (typeof(TKind) == typeof(AttrAttribute))
        {
            return (IReadOnlyCollection<TKind>)resourceType.Attributes;
        }

        if (typeof(TKind) == typeof(RelationshipAttribute))
        {
            return (IReadOnlyCollection<TKind>)resourceType.Relationships;
        }

        return (IReadOnlyCollection<TKind>)resourceType.Fields;
    }

    private IEnumerable<string> ToMemberNames<TResource>(Expression<Func<TResource, object?>> selector)
    {
        Expression selectorBody = RemoveConvert(selector.Body);

        if (selectorBody is MemberExpression memberExpression)
        {
            // model => model.Field1

            yield return memberExpression.Member.Name;
        }
        else if (selectorBody is NewExpression newExpression)
        {
            // model => new { model.Field1, model.Field2 }

            foreach (MemberInfo member in newExpression.Members ?? Enumerable.Empty<MemberInfo>())
            {
                yield return member.Name;
            }
        }
        else
        {
            throw new ArgumentException(
                $"The expression '{selector}' should select a single property or select multiple properties into an anonymous type. " +
                "For example: 'article => article.Title' or 'article => new { article.Title, article.PageCount }'.", nameof(selector));
        }
    }

    private static Expression RemoveConvert(Expression expression)
    {
        Expression innerExpression = expression;

        while (true)
        {
            if (innerExpression is UnaryExpression { NodeType: ExpressionType.Convert } unaryExpression)
            {
                innerExpression = unaryExpression.Operand;
            }
            else
            {
                return innerExpression;
            }
        }
    }
}
