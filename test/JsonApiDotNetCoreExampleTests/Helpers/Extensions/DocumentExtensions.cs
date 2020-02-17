using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCoreExampleTests.Helpers.Extensions
{
    public static class DocumentExtensions
    {
        public static ResourceObject FindResource<TId>(this List<ResourceObject> included, string type, TId id)
        {
            var document = included.FirstOrDefault(documentData =>
                documentData.Type == type && documentData.Id == id.ToString());

            return document;
        }

        public static int CountOfType(this List<ResourceObject> included, string type) {
            return included.Count(documentData => documentData.Type == type);
        }
    }
}
