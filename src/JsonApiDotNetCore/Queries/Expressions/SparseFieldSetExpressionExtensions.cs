using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Expressions
{
    [PublicAPI]
    public static class SparseFieldSetExpressionExtensions
    {
        public static SparseFieldSetExpression Including<TResource>(this SparseFieldSetExpression sparseFieldSet,
            Expression<Func<TResource, dynamic>> fieldSelector, IResourceGraph resourceGraph)
            where TResource : class, IIdentifiable
        {
            ArgumentGuard.NotNull(fieldSelector, nameof(fieldSelector));
            ArgumentGuard.NotNull(resourceGraph, nameof(resourceGraph));

            SparseFieldSetExpression newSparseFieldSet = sparseFieldSet;

            foreach (ResourceFieldAttribute field in resourceGraph.GetFields(fieldSelector))
            {
                newSparseFieldSet = IncludeField(newSparseFieldSet, field);
            }

            return newSparseFieldSet;
        }

        private static SparseFieldSetExpression IncludeField(SparseFieldSetExpression sparseFieldSet, ResourceFieldAttribute fieldToInclude)
        {
            if (sparseFieldSet == null || sparseFieldSet.Fields.Contains(fieldToInclude))
            {
                return sparseFieldSet;
            }

            HashSet<ResourceFieldAttribute> fieldSet = sparseFieldSet.Fields.ToHashSet();
            fieldSet.Add(fieldToInclude);
            return new SparseFieldSetExpression(fieldSet);
        }

        public static SparseFieldSetExpression Excluding<TResource>(this SparseFieldSetExpression sparseFieldSet,
            Expression<Func<TResource, dynamic>> fieldSelector, IResourceGraph resourceGraph)
            where TResource : class, IIdentifiable
        {
            ArgumentGuard.NotNull(fieldSelector, nameof(fieldSelector));
            ArgumentGuard.NotNull(resourceGraph, nameof(resourceGraph));

            SparseFieldSetExpression newSparseFieldSet = sparseFieldSet;

            foreach (ResourceFieldAttribute field in resourceGraph.GetFields(fieldSelector))
            {
                newSparseFieldSet = ExcludeField(newSparseFieldSet, field);
            }

            return newSparseFieldSet;
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

            HashSet<ResourceFieldAttribute> fieldSet = sparseFieldSet.Fields.ToHashSet();
            fieldSet.Remove(fieldToExclude);
            return new SparseFieldSetExpression(fieldSet);
        }
    }
}
