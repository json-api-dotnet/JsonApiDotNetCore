using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.AtomicOperations
{
    public static class OperationContainerExtensions
    {
        public static ISet<IIdentifiable> GetSecondaryResourceIds(this OperationContainer operation)
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));

            var relationship = operation.Request.Relationship;
            var rightValue = relationship.GetValue(operation.Resource);

            var rightResources = TypeHelper.ExtractResources(rightValue);
            return rightResources.ToHashSet(IdentifiableComparer.Instance);
        }

        public static object GetSecondaryResourceIdOrIds(this OperationContainer operation)
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));

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
