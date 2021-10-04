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
        private readonly Lazy<IDictionary<ResourceContext, IImmutableSet<ResourceFieldAttribute>>> _lazySourceTable;
        private readonly IDictionary<ResourceContext, IImmutableSet<ResourceFieldAttribute>> _visitedTable;

        public SparseFieldSetCache(IEnumerable<IQueryConstraintProvider> constraintProviders, IResourceDefinitionAccessor resourceDefinitionAccessor)
        {
            ArgumentGuard.NotNull(constraintProviders, nameof(constraintProviders));
            ArgumentGuard.NotNull(resourceDefinitionAccessor, nameof(resourceDefinitionAccessor));

            _resourceDefinitionAccessor = resourceDefinitionAccessor;
            _lazySourceTable = new Lazy<IDictionary<ResourceContext, IImmutableSet<ResourceFieldAttribute>>>(() => BuildSourceTable(constraintProviders));
            _visitedTable = new Dictionary<ResourceContext, IImmutableSet<ResourceFieldAttribute>>();
        }

        private static IDictionary<ResourceContext, IImmutableSet<ResourceFieldAttribute>> BuildSourceTable(
            IEnumerable<IQueryConstraintProvider> constraintProviders)
        {
            // @formatter:wrap_chained_method_calls chop_always
            // @formatter:keep_existing_linebreaks true

            KeyValuePair<ResourceContext, SparseFieldSetExpression>[] sparseFieldTables = constraintProviders
                .SelectMany(provider => provider.GetConstraints())
                .Where(constraint => constraint.Scope == null)
                .Select(constraint => constraint.Expression)
                .OfType<SparseFieldTableExpression>()
                .Select(expression => expression.Table)
                .SelectMany(table => table)
                .ToArray();

            // @formatter:keep_existing_linebreaks restore
            // @formatter:wrap_chained_method_calls restore

            var mergedTable = new Dictionary<ResourceContext, ImmutableHashSet<ResourceFieldAttribute>.Builder>();

            foreach ((ResourceContext resourceContext, SparseFieldSetExpression sparseFieldSet) in sparseFieldTables)
            {
                if (!mergedTable.ContainsKey(resourceContext))
                {
                    mergedTable[resourceContext] = ImmutableHashSet.CreateBuilder<ResourceFieldAttribute>();
                }

                AddSparseFieldsToSet(sparseFieldSet.Fields, mergedTable[resourceContext]);
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
        public IImmutableSet<ResourceFieldAttribute> GetSparseFieldSetForQuery(ResourceContext resourceContext)
        {
            ArgumentGuard.NotNull(resourceContext, nameof(resourceContext));

            if (!_visitedTable.ContainsKey(resourceContext))
            {
                SparseFieldSetExpression inputExpression = _lazySourceTable.Value.ContainsKey(resourceContext)
                    ? new SparseFieldSetExpression(_lazySourceTable.Value[resourceContext])
                    : null;

                SparseFieldSetExpression outputExpression = _resourceDefinitionAccessor.OnApplySparseFieldSet(resourceContext.ResourceType, inputExpression);

                IImmutableSet<ResourceFieldAttribute> outputFields = outputExpression == null
                    ? ImmutableHashSet<ResourceFieldAttribute>.Empty
                    : outputExpression.Fields;

                _visitedTable[resourceContext] = outputFields;
            }

            return _visitedTable[resourceContext];
        }

        /// <inheritdoc />
        public IImmutableSet<AttrAttribute> GetIdAttributeSetForRelationshipQuery(ResourceContext resourceContext)
        {
            ArgumentGuard.NotNull(resourceContext, nameof(resourceContext));

            AttrAttribute idAttribute = resourceContext.GetAttributeByPropertyName(nameof(Identifiable.Id));
            var inputExpression = new SparseFieldSetExpression(ImmutableHashSet.Create<ResourceFieldAttribute>(idAttribute));

            // Intentionally not cached, as we are fetching ID only (ignoring any sparse fieldset that came from query string).
            SparseFieldSetExpression outputExpression = _resourceDefinitionAccessor.OnApplySparseFieldSet(resourceContext.ResourceType, inputExpression);

            ImmutableHashSet<AttrAttribute> outputAttributes = outputExpression == null
                ? ImmutableHashSet<AttrAttribute>.Empty
                : outputExpression.Fields.OfType<AttrAttribute>().ToImmutableHashSet();

            outputAttributes = outputAttributes.Add(idAttribute);
            return outputAttributes;
        }

        /// <inheritdoc />
        public IImmutableSet<ResourceFieldAttribute> GetSparseFieldSetForSerializer(ResourceContext resourceContext)
        {
            ArgumentGuard.NotNull(resourceContext, nameof(resourceContext));

            if (!_visitedTable.ContainsKey(resourceContext))
            {
                IImmutableSet<ResourceFieldAttribute> inputFields = _lazySourceTable.Value.ContainsKey(resourceContext)
                    ? _lazySourceTable.Value[resourceContext]
                    : GetResourceFields(resourceContext);

                var inputExpression = new SparseFieldSetExpression(inputFields);
                SparseFieldSetExpression outputExpression = _resourceDefinitionAccessor.OnApplySparseFieldSet(resourceContext.ResourceType, inputExpression);

                IImmutableSet<ResourceFieldAttribute> outputFields =
                    outputExpression == null ? GetResourceFields(resourceContext) : inputFields.Intersect(outputExpression.Fields);

                _visitedTable[resourceContext] = outputFields;
            }

            return _visitedTable[resourceContext];
        }

        private IImmutableSet<ResourceFieldAttribute> GetResourceFields(ResourceContext resourceContext)
        {
            ArgumentGuard.NotNull(resourceContext, nameof(resourceContext));

            ImmutableHashSet<ResourceFieldAttribute>.Builder fieldSetBuilder = ImmutableHashSet.CreateBuilder<ResourceFieldAttribute>();

            foreach (AttrAttribute attribute in resourceContext.Attributes.Where(attr => attr.Capabilities.HasFlag(AttrCapabilities.AllowView)))
            {
                fieldSetBuilder.Add(attribute);
            }

            fieldSetBuilder.AddRange(resourceContext.Relationships);

            return fieldSetBuilder.ToImmutable();
        }

        public void Reset()
        {
            _visitedTable.Clear();
        }
    }
}
