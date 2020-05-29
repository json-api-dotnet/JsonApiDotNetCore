using JsonApiDotNetCore.Internal.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal.Queries;
using JsonApiDotNetCore.Internal.Queries.Expressions;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Annotation;
using JsonApiDotNetCore.Query;

namespace JsonApiDotNetCore.Serialization.Server
{
    /// <inheritdoc/>
    public class FieldsToSerialize : IFieldsToSerialize
    {
        private readonly IResourceGraph _resourceGraph;
        private readonly IEnumerable<IQueryConstraintProvider> _constraintProviders;
        private readonly IResourceDefinitionProvider _resourceDefinitionProvider;

        public FieldsToSerialize(
            IResourceGraph resourceGraph,
            IEnumerable<IQueryConstraintProvider> constraintProviders,
            IResourceDefinitionProvider resourceDefinitionProvider)
        {
            _resourceGraph = resourceGraph;
            _constraintProviders = constraintProviders;
            _resourceDefinitionProvider = resourceDefinitionProvider;
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<AttrAttribute> GetAttributes(Type type, RelationshipAttribute relationship = null)
        {   
            var sparseFieldSetAttributes = _constraintProviders
                .SelectMany(p => p.GetConstraints())
                .Where(expressionInScope => relationship == null
                    ? expressionInScope.Scope == null
                    : expressionInScope.Scope != null && expressionInScope.Scope.Fields.Last().Equals(relationship))
                .Select(expressionInScope => expressionInScope.Expression)
                .OfType<SparseFieldSetExpression>()
                .SelectMany(sparseFieldSet => sparseFieldSet.Attributes)
                .ToHashSet();

            if (!sparseFieldSetAttributes.Any())
            {
                sparseFieldSetAttributes = _resourceGraph.GetAttributes(type).ToHashSet();
            }

            sparseFieldSetAttributes.RemoveWhere(attr => !attr.Capabilities.HasFlag(AttrCapabilities.AllowView));

            var resourceDefinition = _resourceDefinitionProvider.Get(type);
            if (resourceDefinition != null)
            {
                var tempExpression = sparseFieldSetAttributes.Any() ? new SparseFieldSetExpression(sparseFieldSetAttributes) : null;
                tempExpression = resourceDefinition.OnApplySparseFieldSet(tempExpression);

                sparseFieldSetAttributes = tempExpression == null ? new HashSet<AttrAttribute>() : tempExpression.Attributes.ToHashSet();
            }

            return sparseFieldSetAttributes;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Note: this method does NOT check if a relationship is included to determine
        /// if it should be serialized. This is because completely hiding a relationship
        /// is not the same as not including. In the case of the latter,
        /// we may still want to add the relationship to expose the navigation link to the client.
        /// </remarks>
        public IReadOnlyCollection<RelationshipAttribute> GetRelationships(Type type)
        {
            return _resourceGraph.GetRelationships(type);
        }
    }
}
