using System;
using System.Linq;
using System.Linq.Expressions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Expressions
{
    public static class SparseFieldSetExpressionExtensions
    {
        public static SparseFieldSetExpression Including<TResource>(this SparseFieldSetExpression sparseFieldSet,
            Expression<Func<TResource, dynamic>> attributeSelector, IResourceGraph resourceGraph)
            where TResource : class, IIdentifiable
        {
            if (attributeSelector == null)
            {
                throw new ArgumentNullException(nameof(attributeSelector));
            }

            if (resourceGraph == null)
            {
                throw new ArgumentNullException(nameof(resourceGraph));
            }

            foreach (var attribute in resourceGraph.GetAttributes(attributeSelector))
            {
                sparseFieldSet = IncludeAttribute(sparseFieldSet, attribute);
            }

            return sparseFieldSet;
        }

        private static SparseFieldSetExpression IncludeAttribute(SparseFieldSetExpression sparseFieldSet, AttrAttribute attributeToInclude)
        {
            if (sparseFieldSet == null || sparseFieldSet.Attributes.Contains(attributeToInclude))
            {
                return sparseFieldSet;
            }

            var attributeSet = sparseFieldSet.Attributes.ToHashSet();
            attributeSet.Add(attributeToInclude);
            return new SparseFieldSetExpression(attributeSet);
        }

        public static SparseFieldSetExpression Excluding<TResource>(this SparseFieldSetExpression sparseFieldSet,
            Expression<Func<TResource, dynamic>> attributeSelector, IResourceGraph resourceGraph)
            where TResource : class, IIdentifiable
        {
            if (attributeSelector == null)
            {
                throw new ArgumentNullException(nameof(attributeSelector));
            }

            if (resourceGraph == null)
            {
                throw new ArgumentNullException(nameof(resourceGraph));
            }

            foreach (var attribute in resourceGraph.GetAttributes(attributeSelector))
            {
                sparseFieldSet = ExcludeAttribute(sparseFieldSet, attribute);
            }

            return sparseFieldSet;
        }

        private static SparseFieldSetExpression ExcludeAttribute(SparseFieldSetExpression sparseFieldSet, AttrAttribute attributeToExclude)
        {
            // Design tradeoff: When the sparse fieldset is empty, it means all attributes will be selected.
            // Adding an exclusion in that case is a no-op, which results in still retrieving the excluded attribute from data store.
            // But later, when serializing the response, the sparse fieldset is first populated with all attributes,
            // so then the exclusion will actually be applied and the excluded attribute is not returned to the client.

            if (sparseFieldSet == null || !sparseFieldSet.Attributes.Contains(attributeToExclude))
            {
                return sparseFieldSet;
            }

            var attributeSet = sparseFieldSet.Attributes.ToHashSet();
            attributeSet.Remove(attributeToExclude);
            return new SparseFieldSetExpression(attributeSet);
        }
    }
}
