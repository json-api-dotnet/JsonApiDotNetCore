using System;
using System.ComponentModel;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Issue988
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class EngagementPartyResourceDefinition : JsonApiResourceDefinition<EngagementParty, Guid>
    {
        /// <inheritdoc />
        public EngagementPartyResourceDefinition(IResourceGraph resourceGraph)
            : base(resourceGraph)
        {
        }

        /// <inheritdoc />
        public override SortExpression OnApplySort(SortExpression? existingSort)
        {
            if (existingSort != null)
            {
                return existingSort;
            }

            return CreateSortExpressionFromLambda(new PropertySortOrder
            {
                (ep => ep.Role, ListSortDirection.Ascending),
                (ep => ep.ShortName, ListSortDirection.Ascending)
            });
        }
    }
}
