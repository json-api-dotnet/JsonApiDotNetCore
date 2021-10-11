using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Internal
{
    /// <inheritdoc />
    public sealed class SparseFieldSetCache : ISparseFieldSetCache
    {
        private readonly IResourceDefinitionAccessor _resourceDefinitionAccessor;
        private readonly Lazy<IDictionary<ResourceType, IImmutableSet<ResourceFieldAttribute>>> _lazySourceTable;
        private readonly IDictionary<ResourceType, IImmutableSet<ResourceFieldAttribute>> _visitedTable;

        public SparseFieldSetCache(IEnumerable<IQueryConstraintProvider> constraintProviders, IResourceDefinitionAccessor resourceDefinitionAccessor)
        {
            ArgumentGuard.NotNull(constraintProviders, nameof(constraintProviders));
            ArgumentGuard.NotNull(resourceDefinitionAccessor, nameof(resourceDefinitionAccessor));

            _resourceDefinitionAccessor = resourceDefinitionAccessor;
            _lazySourceTable = new Lazy<IDictionary<ResourceType, IImmutableSet<ResourceFieldAttribute>>>(() => BuildSourceTable(constraintProviders));
            _visitedTable = new Dictionary<ResourceType, IImmutableSet<ResourceFieldAttribute>>();
        }

        private static IDictionary<ResourceType, IImmutableSet<ResourceFieldAttribute>> BuildSourceTable(
            IEnumerable<IQueryConstraintProvider> constraintProviders)
        {
            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:keep_existing_linebreaks true

            KeyValuePair<ResourceType, SparseFieldSetExpression>[] sparseFieldTables = constraintProviders
                .SelectMany(provider => provider.GetConstraints())
                .Where(constraint => constraint.Scope == null)
                .Select(constraint => constraint.Expression)
                .OfType<SparseFieldTableExpression>()
                .Select(expression => expression.Table)
                .SelectMany(table => table)
                .ToArray();

            // @formatter:keep_existing_linebreaks restore
            // @formatter:wrap_chained_method_calls restore

            var mergedTable = new Dictionary<ResourceType, ImmutableHashSet<ResourceFieldAttribute>.Builder>();

            foreach ((ResourceType resourceType, SparseFieldSetExpression sparseFieldSet) in sparseFieldTables)
            {
                if (!mergedTable.ContainsKey(resourceType))
                {
                    mergedTable[resourceType] = ImmutableHashSet.CreateBuilder<ResourceFieldAttribute>();
                }

                AddSparseFieldsToSet(sparseFieldSet.Fields, mergedTable[resourceType]);
            }

            return mergedTable.ToDictionary(pair => pair.Key, pair => (IImmutableSet<ResourceFieldAttribute>)pair.Value.ToImmutable());
        }

        private static void AddSparseFieldsToSet(IImmutableSet<ResourceFieldAttribute> sparseFieldsToAdd,
            ImmutableHashSet<ResourceFieldAttribute>.Builder sparseFieldSetBuilder)
        {
            foreach (ResourceFieldAttribute field in sparseFieldsToAdd)
            {
                sparseFieldSetBuilder.Add(field);
            }
        }

        /// <inheritdoc />
        public IImmutableSet<ResourceFieldAttribute> GetSparseFieldSetForQuery(ResourceType resourceType)
        {
            ArgumentGuard.NotNull(resourceType, nameof(resourceType));

            if (!_visitedTable.ContainsKey(resourceType))
            {
                SparseFieldSetExpression? inputExpression = _lazySourceTable.Value.ContainsKey(resourceType)
                    ? new SparseFieldSetExpression(_lazySourceTable.Value[resourceType])
                    : null;

                SparseFieldSetExpression? outputExpression = _resourceDefinitionAccessor.OnApplySparseFieldSet(resourceType, inputExpression);

                IImmutableSet<ResourceFieldAttribute> outputFields = outputExpression == null
                    ? ImmutableHashSet<ResourceFieldAttribute>.Empty
                    : outputExpression.Fields;

                _visitedTable[resourceType] = outputFields;
            }

            return _visitedTable[resourceType];
        }

        /// <inheritdoc />
        public IImmutableSet<AttrAttribute> GetIdAttributeSetForRelationshipQuery(ResourceType resourceType)
        {
            ArgumentGuard.NotNull(resourceType, nameof(resourceType));

            AttrAttribute idAttribute = resourceType.GetAttributeByPropertyName(nameof(Identifiable<object>.Id));
            var inputExpression = new SparseFieldSetExpression(ImmutableHashSet.Create<ResourceFieldAttribute>(idAttribute));

            // Intentionally not cached, as we are fetching ID only (ignoring any sparse fieldset that came from query string).
            SparseFieldSetExpression? outputExpression = _resourceDefinitionAccessor.OnApplySparseFieldSet(resourceType, inputExpression);

            ImmutableHashSet<AttrAttribute> outputAttributes = outputExpression == null
                ? ImmutableHashSet<AttrAttribute>.Empty
                : outputExpression.Fields.OfType<AttrAttribute>().ToImmutableHashSet();

            outputAttributes = outputAttributes.Add(idAttribute);
            return outputAttributes;
        }

        /// <inheritdoc />
        public IImmutableSet<ResourceFieldAttribute> GetSparseFieldSetForSerializer(ResourceType resourceType)
        {
            ArgumentGuard.NotNull(resourceType, nameof(resourceType));

            if (!_visitedTable.ContainsKey(resourceType))
            {
                IImmutableSet<ResourceFieldAttribute> inputFields = _lazySourceTable.Value.ContainsKey(resourceType)
                    ? _lazySourceTable.Value[resourceType]
                    : GetResourceFields(resourceType);

                var inputExpression = new SparseFieldSetExpression(inputFields);
                SparseFieldSetExpression? outputExpression = _resourceDefinitionAccessor.OnApplySparseFieldSet(resourceType, inputExpression);

                IImmutableSet<ResourceFieldAttribute> outputFields =
                    outputExpression == null ? GetResourceFields(resourceType) : inputFields.Intersect(outputExpression.Fields);

                _visitedTable[resourceType] = outputFields;
            }

            return _visitedTable[resourceType];
        }

        private IImmutableSet<ResourceFieldAttribute> GetResourceFields(ResourceType resourceType)
        {
            ArgumentGuard.NotNull(resourceType, nameof(resourceType));

            ImmutableHashSet<ResourceFieldAttribute>.Builder fieldSetBuilder = ImmutableHashSet.CreateBuilder<ResourceFieldAttribute>();

            foreach (AttrAttribute attribute in resourceType.Attributes.Where(attr => attr.Capabilities.HasFlag(AttrCapabilities.AllowView)))
            {
                fieldSetBuilder.Add(attribute);
            }

            fieldSetBuilder.AddRange(resourceType.Relationships);

            return fieldSetBuilder.ToImmutable();
        }

        public void Reset()
        {
            _visitedTable.Clear();
        }
    }
}
