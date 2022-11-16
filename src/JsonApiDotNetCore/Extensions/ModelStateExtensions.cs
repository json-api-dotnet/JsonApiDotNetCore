using System;
using System.Net;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Serialization.Objects;
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
                        collection.Errors.Add(new Error(HttpStatusCode.BadRequest)
                            {
                                Title = modelError.ErrorMessage,
                                Meta = modelError.Exception != null ? ErrorMeta.FromException(modelError.Exception) : null
                            });
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
                        collection.Errors.AddRange(jex.GetError().
                            Errors);
                    else
                        collection.Errors.Add(new Error(HttpStatusCode.UnprocessableEntity)
                        {
                            Title = entry.Key,
                            Detail = modelError.ErrorMessage,
                            Meta = modelError.Exception != null ? ErrorMeta.FromException(modelError.Exception) : null,
                            Source = attrName == null ? null : new {
                                pointer = $"/data/attributes/{attrName}"
                                }
                        });
                }
            }

            return collection;
        }
    }
}
