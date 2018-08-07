using JsonApiDotNetCore.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore.Internal;

namespace JsonApiDotNetCore.Extensions
{
    public static class ModelStateExtensions
    {
        public static ErrorCollection ConvertToErrorCollection<T>(this ModelStateDictionary modelState, IContextGraph contextGraph)
        {
            ErrorCollection collection = new ErrorCollection();
            foreach (var entry in modelState)
            {
                if (entry.Value.Errors.Any() == false)
                    continue;

                foreach (var modelError in entry.Value.Errors)
                {
                    var attrName =contextGraph.GetPublicAttributeName<T>(entry.Key);

                    if (modelError.Exception is JsonApiException jex)
                        collection.Errors.AddRange(jex.GetError().Errors);
                    else
                        collection.Errors.Add(new Error(
                            status: 422,
                            title: entry.Key,
                            detail: modelError.ErrorMessage,
                            meta: modelError.Exception != null ? ErrorMeta.FromException(modelError.Exception) : null,
                            source: new {
                                pointer = $"/data/attributes/{attrName}"
                            }));
                }
            }

            return collection;
        }
    }
}
