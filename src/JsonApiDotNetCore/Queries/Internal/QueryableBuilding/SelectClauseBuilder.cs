using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace JsonApiDotNetCore.Queries.Internal.QueryableBuilding
{
    /// <summary>
    /// Transforms <see cref="SparseFieldSetExpression" /> into
    /// <see cref="Queryable.Select{TSource, TKey}(IQueryable{TSource}, System.Linq.Expressions.Expression{System.Func{TSource,TKey}})" /> calls.
    /// </summary>
    [PublicAPI]
    public class SelectClauseBuilder : QueryClauseBuilder<object>
    {
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

        public Expression ApplySelect(IDictionary<ResourceFieldAttribute, QueryLayer?> selectors, ResourceType resourceType)
        {
            ArgumentGuard.NotNull(selectors, nameof(selectors));

            if (!selectors.Any())
            {
                return _source;
            }

            Expression bodyInitializer = CreateLambdaBodyInitializer(selectors, resourceType, LambdaScope, false);

            LambdaExpression lambda = Expression.Lambda(bodyInitializer, LambdaScope.Parameter);

            return SelectExtensionMethodCall(_source, LambdaScope.Parameter.Type, lambda);
        }

        private Expression CreateLambdaBodyInitializer(IDictionary<ResourceFieldAttribute, QueryLayer?> selectors, ResourceType resourceType,
            LambdaScope lambdaScope, bool lambdaAccessorRequiresTestForNull)
        {
            ICollection<PropertySelector> propertySelectors = ToPropertySelectors(selectors, resourceType, lambdaScope.Accessor.Type);

            MemberBinding[] propertyAssignments =
                propertySelectors.Select(selector => CreatePropertyAssignment(selector, lambdaScope)).Cast<MemberBinding>().ToArray();

            NewExpression newExpression = _resourceFactory.CreateNewExpression(lambdaScope.Accessor.Type);
            Expression memberInit = Expression.MemberInit(newExpression, propertyAssignments);

            if (!lambdaAccessorRequiresTestForNull)
            {
                return memberInit;
            }

            return TestForNull(lambdaScope.Accessor, memberInit);
        }

        private ICollection<PropertySelector> ToPropertySelectors(IDictionary<ResourceFieldAttribute, QueryLayer?> resourceFieldSelectors,
            ResourceType resourceType, Type elementType)
        {
            var propertySelectors = new Dictionary<PropertyInfo, PropertySelector>();

            // If a read-only attribute is selected, its calculated value likely depends on another property, so select all properties.
            bool includesReadOnlyAttribute = resourceFieldSelectors.Any(selector =>
                selector.Key is AttrAttribute attribute && attribute.Property.SetMethod == null);

            // Only selecting relationships implicitly means to select all attributes too.
            bool containsOnlyRelationships = resourceFieldSelectors.All(selector => selector.Key is RelationshipAttribute);

            if (includesReadOnlyAttribute || containsOnlyRelationships)
            {
                IncludeAllProperties(elementType, propertySelectors);
            }

            IncludeFieldSelection(resourceFieldSelectors, propertySelectors);

            IncludeEagerLoads(resourceType, propertySelectors);

            return propertySelectors.Values;
        }

        private void IncludeAllProperties(Type elementType, Dictionary<PropertyInfo, PropertySelector> propertySelectors)
        {
            IEntityType entityModel = _entityModel.GetEntityTypes().Single(type => type.ClrType == elementType);
            IEnumerable<IProperty> entityProperties = entityModel.GetProperties().Where(property => !property.IsShadowProperty()).ToArray();

            foreach (IProperty entityProperty in entityProperties)
            {
                var propertySelector = new PropertySelector(entityProperty.PropertyInfo);
                IncludeWritableProperty(propertySelector, propertySelectors);
            }
        }

        private static void IncludeFieldSelection(IDictionary<ResourceFieldAttribute, QueryLayer?> resourceFieldSelectors,
            Dictionary<PropertyInfo, PropertySelector> propertySelectors)
        {
            foreach ((ResourceFieldAttribute resourceField, QueryLayer? queryLayer) in resourceFieldSelectors)
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

        private MemberAssignment CreatePropertyAssignment(PropertySelector selector, LambdaScope lambdaScope)
        {
            MemberExpression propertyAccess = Expression.Property(lambdaScope.Accessor, selector.Property);

            Expression assignmentRightHandSide = propertyAccess;

            if (selector.NextLayer != null)
            {
                var lambdaScopeFactory = new LambdaScopeFactory(_nameFactory);

                assignmentRightHandSide = CreateAssignmentRightHandSideForLayer(selector.NextLayer, lambdaScope, propertyAccess,
                    selector.Property, lambdaScopeFactory);
            }

            return Expression.Bind(selector.Property, assignmentRightHandSide);
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

            if (layer.Projection.IsNullOrEmpty())
            {
                return propertyAccess;
            }

            using LambdaScope scope = lambdaScopeFactory.CreateScope(bodyElementType, propertyAccess);
            return CreateLambdaBodyInitializer(layer.Projection, layer.ResourceType, scope, true);
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

        private Expression SelectExtensionMethodCall(Expression source, Type elementType, Expression selectorBody)
        {
            Type[] typeArguments = ArrayFactory.Create(elementType, elementType);
            return Expression.Call(_extensionType, "Select", typeArguments, source, selectorBody);
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
}
