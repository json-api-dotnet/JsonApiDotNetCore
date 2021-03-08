using System.Linq;
using JetBrains.Annotations;
using JsonApiDotNetCore;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.SoftDeletion
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public sealed class SoftDeletionResourceDefinition<TResource> : JsonApiResourceDefinition<TResource>
        where TResource : class, IIdentifiable<int>, ISoftDeletable
    {
        private readonly IResourceGraph _resourceGraph;

        public SoftDeletionResourceDefinition(IResourceGraph resourceGraph)
            : base(resourceGraph)
        {
            _resourceGraph = resourceGraph;
        }

        public override FilterExpression OnApplyFilter(FilterExpression existingFilter)
        {
            ResourceContext resourceContext = _resourceGraph.GetResourceContext<TResource>();

            AttrAttribute isSoftDeletedAttribute =
                resourceContext.Attributes.Single(attribute => attribute.Property.Name == nameof(ISoftDeletable.IsSoftDeleted));

            var isNotSoftDeleted = new ComparisonExpression(ComparisonOperator.Equals, new ResourceFieldChainExpression(isSoftDeletedAttribute),
                new LiteralConstantExpression("false"));

            return existingFilter == null
                ? (FilterExpression)isNotSoftDeleted
                : new LogicalExpression(LogicalOperator.And, ArrayFactory.Create(isNotSoftDeleted, existingFilter));
        }
    }
}
