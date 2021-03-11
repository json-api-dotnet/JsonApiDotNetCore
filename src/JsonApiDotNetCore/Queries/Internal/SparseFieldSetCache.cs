using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Internal
{
    /// <summary>
    /// Takes sparse fieldsets from <see cref="IQueryConstraintProvider" />s and invokes
    /// <see cref="IResourceDefinition{TResource,TId}.OnApplySparseFieldSet" /> on them.
    /// </summary>
    [PublicAPI]
    public sealed class SparseFieldSetCache
    {
        private readonly IResourceDefinitionAccessor _resourceDefinitionAccessor;
        private readonly Lazy<IDictionary<ResourceContext, HashSet<ResourceFieldAttribute>>> _lazySourceTable;
        private readonly IDictionary<ResourceContext, HashSet<ResourceFieldAttribute>> _visitedTable;

        public SparseFieldSetCache(IEnumerable<IQueryConstraintProvider> constraintProviders, IResourceDefinitionAccessor resourceDefinitionAccessor)
        {
            ArgumentGuard.NotNull(constraintProviders, nameof(constraintProviders));
            ArgumentGuard.NotNull(resourceDefinitionAccessor, nameof(resourceDefinitionAccessor));

            _resourceDefinitionAccessor = resourceDefinitionAccessor;
            _lazySourceTable = new Lazy<IDictionary<ResourceContext, HashSet<ResourceFieldAttribute>>>(() => BuildSourceTable(constraintProviders));
            _visitedTable = new Dictionary<ResourceContext, HashSet<ResourceFieldAttribute>>();
        }

        private static IDictionary<ResourceContext, HashSet<ResourceFieldAttribute>> BuildSourceTable(IEnumerable<IQueryConstraintProvider> constraintProviders)
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

            var mergedTable = new Dictionary<ResourceContext, HashSet<ResourceFieldAttribute>>();

            foreach ((ResourceContext resourceContext, SparseFieldSetExpression sparseFieldSet) in sparseFieldTables)
            {
                if (!mergedTable.ContainsKey(resourceContext))
                {
                    mergedTable[resourceContext] = new HashSet<ResourceFieldAttribute>();
                }

                AddSparseFieldsToSet(sparseFieldSet.Fields, mergedTable[resourceContext]);
            }

            return mergedTable;
        }

        private static void AddSparseFieldsToSet(IReadOnlyCollection<ResourceFieldAttribute> sparseFieldsToAdd, HashSet<ResourceFieldAttribute> sparseFieldSet)
        {
            foreach (ResourceFieldAttribute field in sparseFieldsToAdd)
            {
                sparseFieldSet.Add(field);
            }
        }

        public IReadOnlyCollection<ResourceFieldAttribute> GetSparseFieldSetForQuery(ResourceContext resourceContext)
        {
            ArgumentGuard.NotNull(resourceContext, nameof(resourceContext));

            if (!_visitedTable.ContainsKey(resourceContext))
            {
                SparseFieldSetExpression inputExpression = _lazySourceTable.Value.ContainsKey(resourceContext)
                    ? new SparseFieldSetExpression(_lazySourceTable.Value[resourceContext])
                    : null;

                SparseFieldSetExpression outputExpression = _resourceDefinitionAccessor.OnApplySparseFieldSet(resourceContext.ResourceType, inputExpression);

                HashSet<ResourceFieldAttribute> outputFields = outputExpression == null
                    ? new HashSet<ResourceFieldAttribute>()
                    : outputExpression.Fields.ToHashSet();

                _visitedTable[resourceContext] = outputFields;
            }

            return _visitedTable[resourceContext];
        }

        public IReadOnlyCollection<AttrAttribute> GetIdAttributeSetForRelationshipQuery(ResourceContext resourceContext)
        {
            ArgumentGuard.NotNull(resourceContext, nameof(resourceContext));

            AttrAttribute idAttribute = resourceContext.Attributes.Single(attr => attr.Property.Name == nameof(Identifiable.Id));
            var inputExpression = new SparseFieldSetExpression(idAttribute.AsArray());

            // Intentionally not cached, as we are fetching ID only (ignoring any sparse fieldset that came from query string).
            SparseFieldSetExpression outputExpression = _resourceDefinitionAccessor.OnApplySparseFieldSet(resourceContext.ResourceType, inputExpression);

            HashSet<AttrAttribute> outputAttributes = outputExpression == null
                ? new HashSet<AttrAttribute>()
                : outputExpression.Fields.OfType<AttrAttribute>().ToHashSet();

            outputAttributes.Add(idAttribute);
            return outputAttributes;
        }

        public IReadOnlyCollection<ResourceFieldAttribute> GetSparseFieldSetForSerializer(ResourceContext resourceContext)
        {
            ArgumentGuard.NotNull(resourceContext, nameof(resourceContext));

            if (!_visitedTable.ContainsKey(resourceContext))
            {
                HashSet<ResourceFieldAttribute> inputFields = _lazySourceTable.Value.ContainsKey(resourceContext)
                    ? _lazySourceTable.Value[resourceContext]
                    : GetResourceFields(resourceContext);

                var inputExpression = new SparseFieldSetExpression(inputFields);
                SparseFieldSetExpression outputExpression = _resourceDefinitionAccessor.OnApplySparseFieldSet(resourceContext.ResourceType, inputExpression);

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

#pragma warning disable AV1130 // Return type in method signature should be a collection interface instead of a concrete type
        private HashSet<ResourceFieldAttribute> GetResourceFields(ResourceContext resourceContext)
#pragma warning restore AV1130 // Return type in method signature should be a collection interface instead of a concrete type
        {
            ArgumentGuard.NotNull(resourceContext, nameof(resourceContext));

            var fieldSet = new HashSet<ResourceFieldAttribute>();

            foreach (AttrAttribute attribute in resourceContext.Attributes.Where(attr => attr.Capabilities.HasFlag(AttrCapabilities.AllowView)))
            {
                fieldSet.Add(attribute);
            }

            foreach (RelationshipAttribute relationship in resourceContext.Relationships)
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
