using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace JsonApiDotNetCore.Queries.Internal.QueryableBuilding;

/// <summary>
/// Transforms <see cref="SparseFieldSetExpression" /> into
/// <see cref="Queryable.Select{TSource, TKey}(IQueryable{TSource}, System.Linq.Expressions.Expression{System.Func{TSource,TKey}})" /> calls.
/// </summary>
[PublicAPI]
public class SelectClauseBuilder : QueryClauseBuilder<object>
{
    private static readonly MethodInfo TypeGetTypeMethod = typeof(object).GetMethod("GetType")!;
    private static readonly MethodInfo TypeOpEqualityMethod = typeof(Type).GetMethod("op_Equality")!;
    private static readonly CollectionConverter CollectionConverter = new();
    private static readonly ConstantExpression NullConstant = Expression.Constant(null);

    private readonly Expression _source;
    private readonly IModel _entityModel;
    private readonly Type _extensionType;
    private readonly LambdaParameterNameFactory _nameFactory;
    private readonly IResourceFactory _resourceFactory;

    public SelectClauseBuilder(Expression source, LambdaScope lambdaScope, IModel entityModel, Type extensionType, LambdaParameterNameFactory nameFactory,
        IResourceFactory resourceFactory)
        : base(lambdaScope)
    {
        ArgumentGuard.NotNull(source, nameof(source));
        ArgumentGuard.NotNull(entityModel, nameof(entityModel));
        ArgumentGuard.NotNull(extensionType, nameof(extensionType));
        ArgumentGuard.NotNull(nameFactory, nameof(nameFactory));
        ArgumentGuard.NotNull(resourceFactory, nameof(resourceFactory));

        _source = source;
        _entityModel = entityModel;
        _extensionType = extensionType;
        _nameFactory = nameFactory;
        _resourceFactory = resourceFactory;
    }

    public Expression ApplySelect(FieldSelection selection, ResourceType resourceType)
    {
        ArgumentGuard.NotNull(selection, nameof(selection));

        Expression bodyInitializer = CreateLambdaBodyInitializer(selection, resourceType, LambdaScope, false);

        LambdaExpression lambda = Expression.Lambda(bodyInitializer, LambdaScope.Parameter);

        return SelectExtensionMethodCall(_source, LambdaScope.Parameter.Type, lambda);
    }

    private Expression CreateLambdaBodyInitializer(FieldSelection selection, ResourceType resourceType, LambdaScope lambdaScope,
        bool lambdaAccessorRequiresTestForNull)
    {
        IEntityType entityType = _entityModel.FindEntityType(resourceType.ClrType)!;
        IEntityType[] concreteEntityTypes = entityType.GetConcreteDerivedTypesInclusive().ToArray();

        Expression bodyInitializer = concreteEntityTypes.Length > 1
            ? CreateLambdaBodyInitializerForTypeHierarchy(selection, resourceType, concreteEntityTypes, lambdaScope)
            : CreateLambdaBodyInitializerForSingleType(selection, resourceType, lambdaScope);

        if (!lambdaAccessorRequiresTestForNull)
        {
            return bodyInitializer;
        }

        return TestForNull(lambdaScope.Accessor, bodyInitializer);
    }

    private Expression CreateLambdaBodyInitializerForTypeHierarchy(FieldSelection selection, ResourceType baseResourceType,
        IEnumerable<IEntityType> concreteEntityTypes, LambdaScope lambdaScope)
    {
        IReadOnlySet<ResourceType> resourceTypes = selection.GetResourceTypes();
        Expression rootCondition = lambdaScope.Accessor;

        foreach (IEntityType entityType in concreteEntityTypes)
        {
            ResourceType? resourceType = resourceTypes.SingleOrDefault(type => type.ClrType == entityType.ClrType);

            if (resourceType != null)
            {
                FieldSelectors fieldSelectors = selection.GetOrCreateSelectors(resourceType);

                if (!fieldSelectors.IsEmpty)
                {
                    ICollection<PropertySelector> propertySelectors = ToPropertySelectors(fieldSelectors, resourceType, entityType.ClrType);

                    MemberBinding[] propertyAssignments = propertySelectors.Select(selector => CreatePropertyAssignment(selector, lambdaScope))
                        .Cast<MemberBinding>().ToArray();

                    NewExpression createInstance = _resourceFactory.CreateNewExpression(entityType.ClrType);
                    MemberInitExpression memberInit = Expression.MemberInit(createInstance, propertyAssignments);
                    UnaryExpression castToBaseType = Expression.Convert(memberInit, baseResourceType.ClrType);

                    BinaryExpression typeCheck = CreateRuntimeTypeCheck(lambdaScope, entityType.ClrType);
                    rootCondition = Expression.Condition(typeCheck, castToBaseType, rootCondition);
                }
            }
        }

        return rootCondition;
    }

    private static BinaryExpression CreateRuntimeTypeCheck(LambdaScope lambdaScope, Type concreteClrType)
    {
        // Emitting "resource.GetType() == typeof(Article)" instead of "resource is Article" so we don't need to check for most-derived
        // types first. This way, we can fallback to "anything else" at the end without worrying about order.

        Expression concreteTypeConstant = concreteClrType.CreateTupleAccessExpressionForConstant(typeof(Type));
        MethodCallExpression getTypeCall = Expression.Call(lambdaScope.Accessor, TypeGetTypeMethod);

        return Expression.MakeBinary(ExpressionType.Equal, getTypeCall, concreteTypeConstant, false, TypeOpEqualityMethod);
    }

    private Expression CreateLambdaBodyInitializerForSingleType(FieldSelection selection, ResourceType resourceType, LambdaScope lambdaScope)
    {
        FieldSelectors fieldSelectors = selection.GetOrCreateSelectors(resourceType);
        ICollection<PropertySelector> propertySelectors = ToPropertySelectors(fieldSelectors, resourceType, lambdaScope.Accessor.Type);

        MemberBinding[] propertyAssignments =
            propertySelectors.Select(selector => CreatePropertyAssignment(selector, lambdaScope)).Cast<MemberBinding>().ToArray();

        NewExpression createInstance = _resourceFactory.CreateNewExpression(lambdaScope.Accessor.Type);
        return Expression.MemberInit(createInstance, propertyAssignments);
    }

    private ICollection<PropertySelector> ToPropertySelectors(FieldSelectors fieldSelectors, ResourceType resourceType, Type elementType)
    {
        var propertySelectors = new Dictionary<PropertyInfo, PropertySelector>();

        if (fieldSelectors.ContainsReadOnlyAttribute || fieldSelectors.ContainsOnlyRelationships)
        {
            // If a read-only attribute is selected, its calculated value likely depends on another property, so select all properties.
            // And only selecting relationships implicitly means to select all attributes too.

            IncludeAllAttributes(elementType, propertySelectors);
        }

        IncludeFields(fieldSelectors, propertySelectors);
        IncludeEagerLoads(resourceType, propertySelectors);

        return propertySelectors.Values;
    }

    private void IncludeAllAttributes(Type elementType, Dictionary<PropertyInfo, PropertySelector> propertySelectors)
    {
        IEntityType entityModel = _entityModel.GetEntityTypes().Single(type => type.ClrType == elementType);
        IEnumerable<IProperty> entityProperties = entityModel.GetProperties().Where(property => !property.IsShadowProperty()).ToArray();

        foreach (IProperty entityProperty in entityProperties)
        {
            var propertySelector = new PropertySelector(entityProperty.PropertyInfo!);
            IncludeWritableProperty(propertySelector, propertySelectors);
        }
    }

    private static void IncludeFields(FieldSelectors fieldSelectors, Dictionary<PropertyInfo, PropertySelector> propertySelectors)
    {
        foreach ((ResourceFieldAttribute resourceField, QueryLayer? queryLayer) in fieldSelectors)
        {
            var propertySelector = new PropertySelector(resourceField.Property, queryLayer);
            IncludeWritableProperty(propertySelector, propertySelectors);
        }
    }

    private static void IncludeWritableProperty(PropertySelector propertySelector, Dictionary<PropertyInfo, PropertySelector> propertySelectors)
    {
        if (propertySelector.Property.SetMethod != null)
        {
            propertySelectors[propertySelector.Property] = propertySelector;
        }
    }

    private static void IncludeEagerLoads(ResourceType resourceType, Dictionary<PropertyInfo, PropertySelector> propertySelectors)
    {
        foreach (EagerLoadAttribute eagerLoad in resourceType.EagerLoads)
        {
            var propertySelector = new PropertySelector(eagerLoad.Property);

            // When an entity navigation property is decorated with both EagerLoadAttribute and RelationshipAttribute,
            // it may already exist with a sub-layer. So do not overwrite in that case.
            if (!propertySelectors.ContainsKey(propertySelector.Property))
            {
                propertySelectors[propertySelector.Property] = propertySelector;
            }
        }
    }

    private MemberAssignment CreatePropertyAssignment(PropertySelector propertySelector, LambdaScope lambdaScope)
    {
        bool requiresUpCast = lambdaScope.Accessor.Type != propertySelector.Property.DeclaringType &&
            lambdaScope.Accessor.Type.IsAssignableFrom(propertySelector.Property.DeclaringType);

        MemberExpression propertyAccess = requiresUpCast
            ? Expression.MakeMemberAccess(Expression.Convert(lambdaScope.Accessor, propertySelector.Property.DeclaringType!), propertySelector.Property)
            : Expression.Property(lambdaScope.Accessor, propertySelector.Property);

        Expression assignmentRightHandSide = propertyAccess;

        if (propertySelector.NextLayer != null)
        {
            var lambdaScopeFactory = new LambdaScopeFactory(_nameFactory);

            assignmentRightHandSide = CreateAssignmentRightHandSideForLayer(propertySelector.NextLayer, lambdaScope, propertyAccess, propertySelector.Property,
                lambdaScopeFactory);
        }

        return Expression.Bind(propertySelector.Property, assignmentRightHandSide);
    }

    private Expression CreateAssignmentRightHandSideForLayer(QueryLayer layer, LambdaScope outerLambdaScope, MemberExpression propertyAccess,
        PropertyInfo selectorPropertyInfo, LambdaScopeFactory lambdaScopeFactory)
    {
        Type? collectionElementType = CollectionConverter.FindCollectionElementType(selectorPropertyInfo.PropertyType);
        Type bodyElementType = collectionElementType ?? selectorPropertyInfo.PropertyType;

        if (collectionElementType != null)
        {
            return CreateCollectionInitializer(outerLambdaScope, selectorPropertyInfo, bodyElementType, layer, lambdaScopeFactory);
        }

        if (layer.Selection == null || layer.Selection.IsEmpty)
        {
            return propertyAccess;
        }

        using LambdaScope scope = lambdaScopeFactory.CreateScope(bodyElementType, propertyAccess);
        return CreateLambdaBodyInitializer(layer.Selection, layer.ResourceType, scope, true);
    }

    private Expression CreateCollectionInitializer(LambdaScope lambdaScope, PropertyInfo collectionProperty, Type elementType, QueryLayer layer,
        LambdaScopeFactory lambdaScopeFactory)
    {
        MemberExpression propertyExpression = Expression.Property(lambdaScope.Accessor, collectionProperty);

        var builder = new QueryableBuilder(propertyExpression, elementType, typeof(Enumerable), _nameFactory, _resourceFactory, _entityModel,
            lambdaScopeFactory);

        Expression layerExpression = builder.ApplyQuery(layer);

        string operationName = CollectionConverter.TypeCanContainHashSet(collectionProperty.PropertyType) ? "ToHashSet" : "ToList";
        return CopyCollectionExtensionMethodCall(layerExpression, operationName, elementType);
    }

    private static Expression TestForNull(Expression expressionToTest, Expression ifFalseExpression)
    {
        BinaryExpression equalsNull = Expression.Equal(expressionToTest, NullConstant);
        return Expression.Condition(equalsNull, Expression.Convert(NullConstant, expressionToTest.Type), ifFalseExpression);
    }

    private static Expression CopyCollectionExtensionMethodCall(Expression source, string operationName, Type elementType)
    {
        return Expression.Call(typeof(Enumerable), operationName, elementType.AsArray(), source);
    }

    private Expression SelectExtensionMethodCall(Expression source, Type elementType, Expression selectBody)
    {
        Type[] typeArguments = ArrayFactory.Create(elementType, elementType);
        return Expression.Call(_extensionType, "Select", typeArguments, source, selectBody);
    }

    private sealed class PropertySelector
    {
        public PropertyInfo Property { get; }
        public QueryLayer? NextLayer { get; }

        public PropertySelector(PropertyInfo property, QueryLayer? nextLayer = null)
        {
            ArgumentGuard.NotNull(property, nameof(property));

            Property = property;
            NextLayer = nextLayer;
        }

        public override string ToString()
        {
            return $"Property: {(NextLayer != null ? $"{Property.Name}..." : Property.Name)}";
        }
    }
}
