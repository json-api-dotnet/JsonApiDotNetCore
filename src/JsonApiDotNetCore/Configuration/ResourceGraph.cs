using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using JsonApiDotNetCore.Queries.Expressions;
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

    /// <inheritdoc />
    public ResourceFieldChainExpression GetFieldChain<TResource>(Expression<Func<TResource, object?>> selector)
        where TResource : class, IIdentifiable
    {
        ArgumentNullException.ThrowIfNull(selector);

        IReadOnlyCollection<ResourceFieldChainExpression> chains = GetFieldChains(selector);

        if (chains.Count != 1)
        {
            throw new ArgumentException($"The expression '{selector}' must select a single property or chain.", nameof(selector));
        }

        return chains.First();
    }

    /// <inheritdoc />
    public IReadOnlyCollection<ResourceFieldChainExpression> GetFieldChains<TResource>(Expression<Func<TResource, object?>> selector)
        where TResource : class, IIdentifiable
    {
        ArgumentNullException.ThrowIfNull(selector);

        ResourceType resourceType = GetResourceType<TResource>();
        return ResolveFieldChains(selector, resourceType);
    }

    private ReadOnlyCollection<ResourceFieldChainExpression> ResolveFieldChains<TResource>(Expression<Func<TResource, object?>> selector,
        ResourceType resourceType)
    {
        Expression selectorBody = RemoveConvert(selector.Body);
        ParameterExpression parameter = selector.Parameters[0];

        List<List<MemberInfo>> memberChains = [];

        if (selectorBody is MemberExpression memberExpression)
        {
            memberChains.Add(ExtractMemberChain(memberExpression, parameter, selector));
        }
        else if (selectorBody is NewExpression newExpression)
        {
            foreach (Expression argument in newExpression.Arguments)
            {
                Expression argumentBody = RemoveConvert(argument);

                if (argumentBody is not MemberExpression argMemberExpr)
                {
                    throw new ArgumentException($"The expression '{selector}' contains an invalid member selection. Expected member access expression.",
                        nameof(selector));
                }

                memberChains.Add(ExtractMemberChain(argMemberExpr, parameter, selector));
            }
        }
        else
        {
            throw new ArgumentException(
                $"The expression '{selector}' should select a single property or chain, or select multiple properties into an anonymous type. " +
                "For example: 'article => article.Title' or 'article => article.Details.Nested.Description' or 'article => new { article.Title, article.Details.Nested.Description }'.",
                nameof(selector));
        }

        List<ResourceFieldChainExpression> fieldChains = [];

        foreach (List<MemberInfo> memberChain in memberChains)
        {
            fieldChains.Add(ResolveSingleFieldChain(memberChain, resourceType, selector));
        }

        return fieldChains.AsReadOnly();
    }

    private static List<MemberInfo> ExtractMemberChain(MemberExpression memberExpression, ParameterExpression parameter, Expression selector)
    {
        List<MemberInfo> members = [];
        Expression? current = memberExpression;

        while (current != null)
        {
            current = RemoveConvert(current);

            if (current == parameter)
            {
                members.Reverse();
                return members;
            }

            if (current is MemberExpression memberExpr)
            {
                members.Add(memberExpr.Member);
                current = memberExpr.Expression;
            }
            else
            {
                throw new ArgumentException(
                    $"The expression '{selector}' contains an invalid expression '{current}'. Expected property member access on '{parameter.Name}'.",
                    nameof(selector));
            }
        }

        throw new ArgumentException($"The expression '{selector}' does not originate from parameter '{parameter.Name}'.", nameof(selector));
    }

    private static ResourceFieldChainExpression ResolveSingleFieldChain(List<MemberInfo> members, ResourceType resourceType, Expression selector)
    {
        List<ResourceFieldAttribute> fields = [];
        IAttributeContainer currentContainer = resourceType;

        for (int index = 0; index < members.Count; index++)
        {
            MemberInfo member = members[index];
            ResourceFieldAttribute? field = null;

            if (currentContainer is ResourceType currentResourceType)
            {
                field = currentResourceType.FindAttributeByPropertyName(member.Name) ??
                    (ResourceFieldAttribute?)currentResourceType.FindRelationshipByPropertyName(member.Name);
            }
            else if (currentContainer is CompoundType compoundType)
            {
                field = compoundType.FindAttributeByPropertyName(member.Name);
            }

            if (field == null)
            {
                string containerName = currentContainer is ResourceType resType ? resType.PublicName : currentContainer.ClrType.Name;

                throw new ArgumentException(
                    $"The expression '{selector}' contains member '{member.Name}', which is not exposed as a JSON:API field on '{containerName}'.",
                    nameof(selector));
            }

            fields.Add(field);

            if (index < members.Count - 1)
            {
                if (field is AttrAttribute { IsCompound: true, CompoundType: not null } compoundAttr)
                {
                    currentContainer = compoundAttr.CompoundType;
                }
                else if (field is RelationshipAttribute relationshipAttr)
                {
                    currentContainer = relationshipAttr.RightType;
                }
                else
                {
                    throw new ArgumentException(
                        $"The expression '{selector}' contains member '{field.Property.Name}', which cannot have nested fields because it is neither a compound attribute nor a relationship.",
                        nameof(selector));
                }
            }
        }

        return new ResourceFieldChainExpression(fields.ToImmutableArray());
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
