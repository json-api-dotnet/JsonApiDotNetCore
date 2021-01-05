using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.AtomicOperations.Processors
{
    public abstract class BaseRelationshipProcessor
    {
        protected ISet<IIdentifiable> GetSecondaryResourceIds(OperationContainer operation)
        {
            var relationship = operation.Request.Relationship;
            var rightValue = relationship.GetValue(operation.Resource);

            var rightResources = TypeHelper.ExtractResources(rightValue);
            return rightResources.ToHashSet(IdentifiableComparer.Instance);
        }

        protected object GetSecondaryResourceIdOrIds(OperationContainer operation)
        {
            var relationship = operation.Request.Relationship;
            var rightValue = relationship.GetValue(operation.Resource);

            if (relationship is HasManyAttribute)
            {
                var rightResources = TypeHelper.ExtractResources(rightValue);
                return rightResources.ToHashSet(IdentifiableComparer.Instance);
            }

            return rightValue;
        }
    }
}
