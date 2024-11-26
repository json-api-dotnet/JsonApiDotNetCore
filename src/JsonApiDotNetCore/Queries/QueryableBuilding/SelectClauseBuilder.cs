using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace JsonApiDotNetCore.Queries.QueryableBuilding;

/// <inheritdoc cref="ISelectClauseBuilder" />
[PublicAPI]
public class SelectClauseBuilder : QueryClauseBuilder, ISelectClauseBuilder
{
    private static readonly MethodInfo TypeGetTypeMethod = typeof(object).GetMethod("GetType")!;
    private static readonly MethodInfo TypeOpEqualityMethod = typeof(Type).GetMethod("op_Equality")!;
    private static readonly ConstantExpression NullConstant = Expression.Constant(null);

    private readonly IResourceFactory _resourceFactory;

    public SelectClauseBuilder(IResourceFactory resourceFactory)
    {
        ArgumentGuard.NotNull(resourceFactory);

        _resourceFactory = resourceFactory;
    }

    public virtual Expression ApplySelect(FieldSelection selection, QueryClauseBuilderContext context)
    {
        ArgumentGuard.NotNull(selection);

        Expression bodyInitializer = CreateLambdaBodyInitializer(selection, context.ResourceType, context.LambdaScope, false, context);

        LambdaExpression lambda = Expression.Lambda(bodyInitializer, context.LambdaScope.Parameter);

        return SelectExtensionMethodCall(context.ExtensionType, context.Source, context.LambdaScope.Parameter.Type, lambda);
    }

    private Expression CreateLambdaBodyInitializer(FieldSelection selection, ResourceType resourceType, LambdaScope lambdaScope,
        bool lambdaAccessorRequiresTestForNull, QueryClauseBuilderContext context)
    {
        IReadOnlyEntityType entityType = context.EntityModel.FindEntityType(resourceType.ClrType)!;
        IReadOnlyEntityType[] concreteEntityTypes = entityType.GetConcreteDerivedTypesInclusive().ToArray();

        Expression bodyInitializer = concreteEntityTypes.Length > 1
            ? CreateLambdaBodyInitializerForTypeHierarchy(selection, resourceType, concreteEntityTypes, lambdaScope, context)
            : CreateLambdaBodyInitializerForSingleType(selection, resourceType, lambdaScope, context);

        if (!lambdaAccessorRequiresTestForNull)
        {
            return bodyInitializer;
        }

        return TestForNull(lambdaScope.Accessor, bodyInitializer);
    }

    private Expression CreateLambdaBodyInitializerForTypeHierarchy(FieldSelection selection, ResourceType baseResourceType,
        IEnumerable<IReadOnlyEntityType> concreteEntityTypes, LambdaScope lambdaScope, QueryClauseBuilderContext context)
    {
        IReadOnlySet<ResourceType> resourceTypes = selection.GetResourceTypes();
        Expression rootCondition = lambdaScope.Accessor;

        foreach (IReadOnlyEntityType entityType in concreteEntityTypes)
        {
            ResourceType? resourceType = resourceTypes.SingleOrDefault(type => type.ClrType == entityType.ClrType);

            if (resourceType != null)
            {
                FieldSelectors fieldSelectors = selection.GetOrCreateSelectors(resourceType);

                if (!fieldSelectors.IsEmpty)
                {
                    Dictionary<PropertyInfo, PropertySelector>.ValueCollection propertySelectors =
                        ToPropertySelectors(fieldSelectors, resourceType, entityType.ClrType, context.EntityModel);

                    MemberBinding[] propertyAssignments = propertySelectors.Select(selector => CreatePropertyAssignment(selector, lambdaScope, context))
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
        // types first. This way, we can fall back to "anything else" at the end without worrying about order.

        Expression concreteTypeConstant = SystemExpressionBuilder.CloseOver(concreteClrType);
        MethodCallExpression getTypeCall = Expression.Call(lambdaScope.Accessor, TypeGetTypeMethod);

        return Expression.MakeBinary(ExpressionType.Equal, getTypeCall, concreteTypeConstant, false, TypeOpEqualityMethod);
    }

    private MemberInitExpression CreateLambdaBodyInitializerForSingleType(FieldSelection selection, ResourceType resourceType, LambdaScope lambdaScope,
        QueryClauseBuilderContext context)
    {
        FieldSelectors fieldSelectors = selection.GetOrCreateSelectors(resourceType);

        Dictionary<PropertyInfo, PropertySelector>.ValueCollection propertySelectors =
            ToPropertySelectors(fieldSelectors, resourceType, lambdaScope.Accessor.Type, context.EntityModel);

        MemberBinding[] propertyAssignments = propertySelectors.Select(selector => CreatePropertyAssignment(selector, lambdaScope, context))
            .Cast<MemberBinding>().ToArray();

        NewExpression createInstance = _resourceFactory.CreateNewExpression(lambdaScope.Accessor.Type);
        return Expression.MemberInit(createInstance, propertyAssignments);
    }

    private static Dictionary<PropertyInfo, PropertySelector>.ValueCollection ToPropertySelectors(FieldSelectors fieldSelectors, ResourceType resourceType,
        Type elementType, IReadOnlyModel entityModel)
    {
        var propertySelectors = new Dictionary<PropertyInfo, PropertySelector>();

        if (fieldSelectors.IsEmpty || fieldSelectors.ContainsReadOnlyAttribute || fieldSelectors.ContainsOnlyRelationships)
        {
            // If a read-only attribute is selected, its calculated value likely depends on another property, so fetch all scalar properties.
            // And only selecting relationships implicitly means to fetch all scalar properties as well.
            // Additionally, empty selectors (originating from eliminated includes) indicate to fetch all scalar properties too.

            IncludeAllScalarProperties(elementType, propertySelectors, entityModel);
        }

        IncludeFields(fieldSelectors, propertySelectors);
        IncludeEagerLoads(resourceType, propertySelectors);

        return propertySelectors.Values;
    }

    private static void IncludeAllScalarProperties(Type elementType, Dictionary<PropertyInfo, PropertySelector> propertySelectors, IReadOnlyModel entityModel)
    {
        IReadOnlyEntityType entityType = entityModel.GetEntityTypes().Single(type => type.ClrType == elementType);

        foreach (IReadOnlyProperty property in entityType.GetProperties().Where(property => !property.IsShadowProperty()))
        {
            var propertySelector = new PropertySelector(property.PropertyInfo!);
            IncludeWritableProperty(propertySelector, propertySelectors);
        }

        foreach (IReadOnlyNavigation navigation in entityType.GetNavigations()
            .Where(navigation => navigation.ForeignKey.IsOwnership && !navigation.IsShadowProperty()))
        {
            var propertySelector = new PropertySelector(navigation.PropertyInfo!);
            IncludeWritableProperty(propertySelector, propertySelectors);
        }
    }

    private static void IncludeFields(FieldSelectors fieldSelectors, Dictionary<PropertyInfo, PropertySelector> propertySelectors)
    {
        foreach ((ResourceFieldAttribute resourceField, QueryLayer? nextLayer) in fieldSelectors)
        {
            var propertySelector = new PropertySelector(resourceField.Property, nextLayer);
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
            propertySelectors.TryAdd(propertySelector.Property, propertySelector);
        }
    }

    private MemberAssignment CreatePropertyAssignment(PropertySelector propertySelector, LambdaScope lambdaScope, QueryClauseBuilderContext context)
    {
        bool requiresUpCast = lambdaScope.Accessor.Type != propertySelector.Property.DeclaringType &&
            lambdaScope.Accessor.Type.IsAssignableFrom(propertySelector.Property.DeclaringType);

        MemberExpression propertyAccess = requiresUpCast
            ? Expression.MakeMemberAccess(Expression.Convert(lambdaScope.Accessor, propertySelector.Property.DeclaringType!), propertySelector.Property)
            : Expression.Property(lambdaScope.Accessor, propertySelector.Property);

        Expression assignmentRightHandSide = propertyAccess;

        if (propertySelector.NextLayer != null)
        {
            assignmentRightHandSide =
                CreateAssignmentRightHandSideForLayer(propertySelector.NextLayer, lambdaScope, propertyAccess, propertySelector.Property, context);
        }

        return Expression.Bind(propertySelector.Property, assignmentRightHandSide);
    }

    private Expression CreateAssignmentRightHandSideForLayer(QueryLayer layer, LambdaScope outerLambdaScope, MemberExpression propertyAccess,
        PropertyInfo selectorPropertyInfo, QueryClauseBuilderContext context)
    {
        Type? collectionElementType = CollectionConverter.Instance.FindCollectionElementType(selectorPropertyInfo.PropertyType);
        Type bodyElementType = collectionElementType ?? selectorPropertyInfo.PropertyType;

        if (collectionElementType != null)
        {
            return CreateCollectionInitializer(outerLambdaScope, selectorPropertyInfo, bodyElementType, layer, context);
        }

        if (layer.Selection == null || layer.Selection.IsEmpty)
        {
            return propertyAccess;
        }

        using LambdaScope scope = context.LambdaScopeFactory.CreateScope(bodyElementType, propertyAccess);
        return CreateLambdaBodyInitializer(layer.Selection, layer.ResourceType, scope, true, context);
    }

    private static MethodCallExpression CreateCollectionInitializer(LambdaScope lambdaScope, PropertyInfo collectionProperty, Type elementType,
        QueryLayer layer, QueryClauseBuilderContext context)
    {
        MemberExpression propertyExpression = Expression.Property(lambdaScope.Accessor, collectionProperty);

        var nestedContext = new QueryableBuilderContext(propertyExpression, elementType, typeof(Enumerable), context.EntityModel, context.LambdaScopeFactory,
            context.State);

        Expression layerExpression = context.QueryableBuilder.ApplyQuery(layer, nestedContext);

        string operationName = CollectionConverter.Instance.TypeCanContainHashSet(collectionProperty.PropertyType) ? "ToHashSet" : "ToList";
        return CopyCollectionExtensionMethodCall(layerExpression, operationName, elementType);
    }

    private static ConditionalExpression TestForNull(Expression expressionToTest, Expression ifFalseExpression)
    {
        BinaryExpression equalsNull = Expression.Equal(expressionToTest, NullConstant);
        return Expression.Condition(equalsNull, Expression.Convert(NullConstant, expressionToTest.Type), ifFalseExpression);
    }

    private static MethodCallExpression CopyCollectionExtensionMethodCall(Expression source, string operationName, Type elementType)
    {
        return Expression.Call(typeof(Enumerable), operationName, [elementType], source);
    }

    private static MethodCallExpression SelectExtensionMethodCall(Type extensionType, Expression source, Type elementType, Expression selectBody)
    {
        Type[] typeArguments =
        [
            elementType,
            elementType
        ];

        return Expression.Call(extensionType, "Select", typeArguments, source, selectBody);
    }

    private sealed class PropertySelector
    {
        public PropertyInfo Property { get; }
        public QueryLayer? NextLayer { get; }

        public PropertySelector(PropertyInfo property, QueryLayer? nextLayer = null)
        {
            ArgumentGuard.NotNull(property);

            Property = property;
            NextLayer = nextLayer;
        }

        public override string ToString()
        {
            return $"Property: {(NextLayer != null ? $"{Property.Name}..." : Property.Name)}";
        }
    }
}
