using System;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace JsonApiDotNetCore.Extensions
{
    public static class ModelStateExtensions
    {
        public static ErrorCollection ConvertToErrorCollection<T>(this ModelStateDictionary modelState, Type resourceType)
        {
            ErrorCollection collection = new ErrorCollection();
            foreach (var entry in modelState)
            {
                if (entry.Value.Errors.Any() == false)
                {
                    continue;
                }

                var targetedProperty = resourceType.GetProperty(entry.Key);
                var attrName = targetedProperty.GetCustomAttribute<AttrAttribute>().PublicAttributeName;

                foreach (var modelError in entry.Value.Errors)
                {
                    if (modelError.Exception is JsonApiException jex)
                    {
                        collection.Errors.AddRange(jex.GetError().Errors);
                    }
                    else
                    {
                        collection.Errors.Add(new Error(
                            status: 422,
                            title: entry.Key,
                            detail: modelError.ErrorMessage,
                            meta: modelError.Exception != null ? ErrorMeta.FromException(modelError.Exception) : null,
                            source: attrName == null ? null : new
                            {
                                pointer = $"/data/attributes/{attrName}"
                            }));
                    }
                }
            }
            return collection;
        }
    }
}
