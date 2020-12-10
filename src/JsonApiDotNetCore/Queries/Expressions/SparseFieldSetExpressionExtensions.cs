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
            Expression<Func<TResource, dynamic>> fieldSelector, IResourceGraph resourceGraph)
            where TResource : class, IIdentifiable
        {
            if (fieldSelector == null)
            {
                throw new ArgumentNullException(nameof(fieldSelector));
            }

            if (resourceGraph == null)
            {
                throw new ArgumentNullException(nameof(resourceGraph));
            }

            foreach (var field in resourceGraph.GetFields(fieldSelector))
            {
                sparseFieldSet = IncludeField(sparseFieldSet, field);
            }

            return sparseFieldSet;
        }

        private static SparseFieldSetExpression IncludeField(SparseFieldSetExpression sparseFieldSet, ResourceFieldAttribute fieldToInclude)
        {
            if (sparseFieldSet == null || sparseFieldSet.Fields.Contains(fieldToInclude))
            {
                return sparseFieldSet;
            }

            var fieldSet = sparseFieldSet.Fields.ToHashSet();
            fieldSet.Add(fieldToInclude);
            return new SparseFieldSetExpression(fieldSet);
        }

        public static SparseFieldSetExpression Excluding<TResource>(this SparseFieldSetExpression sparseFieldSet,
            Expression<Func<TResource, dynamic>> fieldSelector, IResourceGraph resourceGraph)
            where TResource : class, IIdentifiable
        {
            if (fieldSelector == null)
            {
                throw new ArgumentNullException(nameof(fieldSelector));
            }

            if (resourceGraph == null)
            {
                throw new ArgumentNullException(nameof(resourceGraph));
            }

            foreach (var field in resourceGraph.GetFields(fieldSelector))
            {
                sparseFieldSet = ExcludeField(sparseFieldSet, field);
            }

            return sparseFieldSet;
        }

        private static SparseFieldSetExpression ExcludeField(SparseFieldSetExpression sparseFieldSet, ResourceFieldAttribute fieldToExclude)
        {
            // Design tradeoff: When the sparse fieldset is empty, it means all fields will be selected.
            // Adding an exclusion in that case is a no-op, which results in still retrieving the excluded field from data store.
            // But later, when serializing the response, the sparse fieldset is first populated with all fields,
            // so then the exclusion will actually be applied and the excluded field is not returned to the client.

            if (sparseFieldSet == null || !sparseFieldSet.Fields.Contains(fieldToExclude))
            {
                return sparseFieldSet;
            }

            var fieldSet = sparseFieldSet.Fields.ToHashSet();
            fieldSet.Remove(fieldToExclude);
            return new SparseFieldSetExpression(fieldSet);
        }
    }
}
