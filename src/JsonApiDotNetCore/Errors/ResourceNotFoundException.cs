using System.Collections.Generic;
using System.Linq;
using System.Net;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Errors
{
    /// <summary>
    /// The error that is thrown when a resource does not exist.
    /// </summary>
    public sealed class ResourceNotFoundException : JsonApiException
    {
        public ResourceNotFoundException(string resourceId, string resourceType) : base(
            new Error(HttpStatusCode.NotFound)
            {
                Title = "The requested resource does not exist.",
                Detail = $"Resource of type '{resourceType}' with ID '{resourceId}' does not exist."
            }) { }

        public ResourceNotFoundException(Dictionary<string, IList<string>> nonExistingResources) : base(
            new Error(HttpStatusCode.NotFound)
            {
                Title = "The requested resources do not exist.",
                Detail = CreateErrorMessageForMultipleMissing(nonExistingResources)
            })
        {
            var pairs = nonExistingResources.ToList();
            if (pairs.Count == 1 && pairs[0].Value.Count == 1)
            {
                var (resourceType, value) = pairs[0];
                var resourceId = value.First();

                throw new ResourceNotFoundException(resourceId, resourceType);
            }
        }

        private static string CreateErrorMessageForMultipleMissing(Dictionary<string, IList<string>> missingResources)
        {
            var errorDetailLines = missingResources.Select(p => $"{p.Key}: {string.Join(',', p.Value)}")
                .ToArray();
            
            return $@"For the following types, the resources with the specified ids do not exist:\n{string.Join('\n', errorDetailLines)}";
        }
    }
}
