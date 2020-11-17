using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Serialization
{
    /// <inheritdoc />
    public class FieldsToSerialize : IFieldsToSerialize
    {
        private readonly IResourceGraph _resourceGraph;
        private readonly IEnumerable<IQueryConstraintProvider> _constraintProviders;
        private readonly IResourceDefinitionAccessor _resourceDefinitionAccessor;
        private readonly IJsonApiRequest _request;

        public FieldsToSerialize(
            IResourceGraph resourceGraph,
            IEnumerable<IQueryConstraintProvider> constraintProviders,
            IResourceDefinitionAccessor resourceDefinitionAccessor,
            IJsonApiRequest request)
        {
            _resourceGraph = resourceGraph ?? throw new ArgumentNullException(nameof(resourceGraph));
            _constraintProviders = constraintProviders ?? throw new ArgumentNullException(nameof(constraintProviders));
            _resourceDefinitionAccessor = resourceDefinitionAccessor ?? throw new ArgumentNullException(nameof(resourceDefinitionAccessor));
            _request = request ?? throw new ArgumentNullException(nameof(request));
        }

        /// <inheritdoc />
        public IReadOnlyCollection<AttrAttribute> GetAttributes(Type resourceType, RelationshipAttribute relationship = null)
        {
            if (resourceType == null) throw new ArgumentNullException(nameof(resourceType));

            if (_request.Kind == EndpointKind.Relationship)
            {
                return Array.Empty<AttrAttribute>();
            }

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
                sparseFieldSetAttributes = GetViewableAttributes(resourceType);
            }

            var inputExpression = sparseFieldSetAttributes.Any() ? new SparseFieldSetExpression(sparseFieldSetAttributes) : null;
            var outputExpression = _resourceDefinitionAccessor.OnApplySparseFieldSet(resourceType, inputExpression);

            if (outputExpression == null)
            {
                sparseFieldSetAttributes = GetViewableAttributes(resourceType);
            }
            else
            {
                sparseFieldSetAttributes.IntersectWith(outputExpression.Attributes);
            }

            return sparseFieldSetAttributes;
        }

        private HashSet<AttrAttribute> GetViewableAttributes(Type resourceType)
        {
            return _resourceGraph.GetAttributes(resourceType)
                .Where(attr => attr.Capabilities.HasFlag(AttrCapabilities.AllowView))
                .ToHashSet();
        }

        /// <inheritdoc />
        /// <remarks>
        /// Note: this method does NOT check if a relationship is included to determine
        /// if it should be serialized. This is because completely hiding a relationship
        /// is not the same as not including. In the case of the latter,
        /// we may still want to add the relationship to expose the navigation link to the client.
        /// </remarks>
        public IReadOnlyCollection<RelationshipAttribute> GetRelationships(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            return _request.Kind == EndpointKind.Relationship
                ? Array.Empty<RelationshipAttribute>()
                : _resourceGraph.GetRelationships(type);
        }
    }
}
