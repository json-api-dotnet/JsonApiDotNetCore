using System.Linq;
using System.Net;
using System.Reflection;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.JsonApiDocuments;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace JsonApiDotNetCore.Extensions
{
    public static class ModelStateExtensions
    {
        public static ErrorCollection ConvertToErrorCollection<T>(this ModelStateDictionary modelState) where T : class, IIdentifiable
        {
            ErrorCollection collection = new ErrorCollection();
            foreach (var entry in modelState)
            {
                if (entry.Value.Errors.Any() == false)
                {
                    continue;
                }

                var targetedProperty = typeof(T).GetProperty(entry.Key);
                var attrName = targetedProperty.GetCustomAttribute<AttrAttribute>().PublicAttributeName;

                foreach (var modelError in entry.Value.Errors)
                {
                    if (modelError.Exception is JsonApiException jex)
                    {
                        collection.Errors.AddRange(jex.GetErrors().Errors);
                    }
                    else
                    {
                        collection.Errors.Add(new Error(
                            status: HttpStatusCode.UnprocessableEntity,
                            title: entry.Key,
                            detail: modelError.ErrorMessage,
                            meta: modelError.Exception != null ? ErrorMeta.FromException(modelError.Exception) : null,
                            source: attrName == null ? null : new ErrorSource
                            {
                                Pointer = $"/data/attributes/{attrName}"
                            }));
                    }
                }
            }
            return collection;
        }
    }
}
