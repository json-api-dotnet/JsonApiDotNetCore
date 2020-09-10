using System.Linq;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.SoftDeletion
{
    public sealed class DepartmentResourceDefinition : ResourceDefinition<Department>
    {
        private readonly IResourceGraph _resourceGraph;

        public DepartmentResourceDefinition(IResourceGraph resourceGraph) : base(resourceGraph)
        {
            _resourceGraph = resourceGraph;
        }

        public override FilterExpression OnApplyFilter(FilterExpression existingFilter)
        {
            var resourceContext = _resourceGraph.GetResourceContext<Department>();
            var isSoftDeletedAttribute = resourceContext.Attributes.Single(attribute => attribute.Property.Name == nameof(Department.IsSoftDeleted));

            var isNotSoftDeleted = new ComparisonExpression(ComparisonOperator.Equals,
                new ResourceFieldChainExpression(isSoftDeletedAttribute), new LiteralConstantExpression("false"));

            return existingFilter == null
                ? (FilterExpression) isNotSoftDeleted
                : new LogicalExpression(LogicalOperator.And, new[] {isNotSoftDeleted, existingFilter});
        }
    }
}
