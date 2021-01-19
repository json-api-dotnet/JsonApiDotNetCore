using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Internal
{
    /// <summary>
    /// Takes sparse fieldsets from <see cref="IQueryConstraintProvider"/>s and invokes <see cref="IResourceDefinition{TResource, TId}.OnApplySparseFieldSet"/> on them.
    /// </summary>
    public sealed class SparseFieldSetCache
    {
        private readonly IResourceDefinitionAccessor _resourceDefinitionAccessor;
        private readonly Lazy<IDictionary<ResourceContext, HashSet<ResourceFieldAttribute>>> _lazySourceTable;
        private readonly IDictionary<ResourceContext, HashSet<ResourceFieldAttribute>> _visitedTable;

        public SparseFieldSetCache(IEnumerable<IQueryConstraintProvider> constraintProviders, IResourceDefinitionAccessor resourceDefinitionAccessor)
        {
            if (constraintProviders == null) throw new ArgumentNullException(nameof(constraintProviders));
            _resourceDefinitionAccessor = resourceDefinitionAccessor ?? throw new ArgumentNullException(nameof(resourceDefinitionAccessor));

            _lazySourceTable = new Lazy<IDictionary<ResourceContext, HashSet<ResourceFieldAttribute>>>(() => BuildSourceTable(constraintProviders));
            _visitedTable = new Dictionary<ResourceContext, HashSet<ResourceFieldAttribute>>();
        }

        private static IDictionary<ResourceContext, HashSet<ResourceFieldAttribute>> BuildSourceTable(IEnumerable<IQueryConstraintProvider> constraintProviders)
        {
            var sparseFieldTables = constraintProviders
                .SelectMany(provider => provider.GetConstraints())
                .Where(constraint => constraint.Scope == null)
                .Select(constraint => constraint.Expression)
                .OfType<SparseFieldTableExpression>()
                .Select(expression => expression.Table)
                .ToArray();

            var mergedTable = new Dictionary<ResourceContext, HashSet<ResourceFieldAttribute>>();

            foreach (var sparseFieldTable in sparseFieldTables)
            {
                foreach (var (resourceContext, sparseFieldSet) in sparseFieldTable)
                {
                    if (!mergedTable.ContainsKey(resourceContext))
                    {
                        mergedTable[resourceContext] = new HashSet<ResourceFieldAttribute>();
                    }

                    foreach (var field in sparseFieldSet.Fields)
                    {
                        mergedTable[resourceContext].Add(field);
                    }
                }
            }

            return mergedTable;
        }

        public IReadOnlyCollection<ResourceFieldAttribute> GetSparseFieldSetForQuery(ResourceContext resourceContext)
        {
            if (resourceContext == null) throw new ArgumentNullException(nameof(resourceContext));

            if (!_visitedTable.ContainsKey(resourceContext))
            {
                var inputExpression = _lazySourceTable.Value.ContainsKey(resourceContext)
                    ? new SparseFieldSetExpression(_lazySourceTable.Value[resourceContext])
                    : null;

                var outputExpression = _resourceDefinitionAccessor.OnApplySparseFieldSet(resourceContext.ResourceType, inputExpression);

                var outputFields = outputExpression == null
                    ? new HashSet<ResourceFieldAttribute>()
                    : outputExpression.Fields.ToHashSet();

                _visitedTable[resourceContext] = outputFields;
            }

            return _visitedTable[resourceContext];
        }

        public IReadOnlyCollection<AttrAttribute> GetIdAttributeSetForRelationshipQuery(ResourceContext resourceContext)
        {
            if (resourceContext == null) throw new ArgumentNullException(nameof(resourceContext));

            var idAttribute = resourceContext.Attributes.Single(attr => attr.Property.Name == nameof(Identifiable.Id));
            var inputExpression = new SparseFieldSetExpression(new []{idAttribute});

            // Intentionally not cached, as we are fetching ID only (ignoring any sparse fieldset that came from query string).
            var outputExpression = _resourceDefinitionAccessor.OnApplySparseFieldSet(resourceContext.ResourceType, inputExpression);

            var outputAttributes = outputExpression == null
                ? new HashSet<AttrAttribute>()
                : outputExpression.Fields.OfType<AttrAttribute>().ToHashSet();

            outputAttributes.Add(idAttribute);
            return outputAttributes;
        }

        public IReadOnlyCollection<ResourceFieldAttribute> GetSparseFieldSetForSerializer(ResourceContext resourceContext)
        {
            if (!_visitedTable.ContainsKey(resourceContext))
            {
                var inputFields = _lazySourceTable.Value.ContainsKey(resourceContext)
                    ? _lazySourceTable.Value[resourceContext]
                    : GetResourceFields(resourceContext);

                var inputExpression = new SparseFieldSetExpression(inputFields);
                var outputExpression = _resourceDefinitionAccessor.OnApplySparseFieldSet(resourceContext.ResourceType, inputExpression);

                HashSet<ResourceFieldAttribute> outputFields;
                if (outputExpression == null)
                {
                    outputFields = GetResourceFields(resourceContext);
                }
                else
                {
                    outputFields = new HashSet<ResourceFieldAttribute>(inputFields);
                    outputFields.IntersectWith(outputExpression.Fields);
                }

                _visitedTable[resourceContext] = outputFields;
            }

            return _visitedTable[resourceContext];
        }

        private HashSet<ResourceFieldAttribute> GetResourceFields(ResourceContext resourceContext)
        {
            if (resourceContext == null) throw new ArgumentNullException(nameof(resourceContext));

            var fieldSet = new HashSet<ResourceFieldAttribute>();

            foreach (var attribute in resourceContext.Attributes.Where(attr => attr.Capabilities.HasFlag(AttrCapabilities.AllowView)))
            {
                fieldSet.Add(attribute);
            }

            foreach (var relationship in resourceContext.Relationships)
            {
                fieldSet.Add(relationship);
            }

            return fieldSet;
        }

        public void Reset()
        {
            _visitedTable.Clear();
        }
    }
}
