using System;
using JsonApiDotNetCore.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore.Internal;

namespace JsonApiDotNetCore.Extensions
{
    public static class ModelStateExtensions
    {
        [Obsolete("Use Generic Method ConvertToErrorCollection<T>(IResourceGraph resourceGraph) instead for full validation errors")]
        public static ErrorCollection ConvertToErrorCollection(this ModelStateDictionary modelState)
        {
            ErrorCollection collection = new ErrorCollection();
            foreach (var entry in modelState)
            {
                if (entry.Value.Errors.Any() == false)
                    continue;

                foreach (var modelError in entry.Value.Errors)
                {
                    if (modelError.Exception is JsonApiException jex)
                        collection.Errors.AddRange(jex.GetError().Errors);
                    else
                        collection.Errors.Add(new Error(400, entry.Key, modelError.ErrorMessage, modelError.Exception != null ? ErrorMeta.FromException(modelError.Exception) : null));
                }
            }

            return collection;
        }
        public static ErrorCollection ConvertToErrorCollection<T>(this ModelStateDictionary modelState, IResourceGraph resourceGraph)
        {
            ErrorCollection collection = new ErrorCollection();
            foreach (var entry in modelState)
            {
                if (entry.Value.Errors.Any() == false)
                    continue;

                var attrName = resourceGraph.GetPublicAttributeName<T>(entry.Key);

                foreach (var modelError in entry.Value.Errors)
                {
                    if (modelError.Exception is JsonApiException jex)
                        collection.Errors.AddRange(jex.GetError().Errors);
                    else
                        collection.Errors.Add(new Error(
                            status: 422,
                            title: entry.Key,
                            detail: modelError.ErrorMessage,
                            meta: modelError.Exception != null ? ErrorMeta.FromException(modelError.Exception) : null,
                            source: attrName == null ? null : new {
                                pointer = $"/data/attributes/{attrName}"
                            }));
                }
            }

            return collection;
        }
    }
}
