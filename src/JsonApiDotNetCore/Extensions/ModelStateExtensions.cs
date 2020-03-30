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
        public static ErrorCollection ConvertToErrorCollection<TResource>(this ModelStateDictionary modelState)
            where TResource : class, IIdentifiable
        {
            ErrorCollection collection = new ErrorCollection();

            foreach (var pair in modelState.Where(x => x.Value.Errors.Any()))
            {
                var propertyName = pair.Key;
                PropertyInfo property = typeof(TResource).GetProperty(propertyName);
                string attributeName = property?.GetCustomAttribute<AttrAttribute>().PublicAttributeName;

                foreach (var modelError in pair.Value.Errors)
                {
                    if (modelError.Exception is JsonApiException jsonApiException)
                    {
                        collection.Errors.AddRange(jsonApiException.GetErrors().Errors);
                    }
                    else
                    {
                        collection.Errors.Add(FromModelError(modelError, propertyName, attributeName));
                    }
                }
            }

            return collection;
        }

        private static Error FromModelError(ModelError modelError, string propertyName, string attributeName)
        {
            return new Error
            {
                Status = HttpStatusCode.UnprocessableEntity,
                Title = "Input validation failed.",
                Detail = propertyName + ": " + modelError.ErrorMessage,
                Source = attributeName == null ? null : new ErrorSource
                {
                    Pointer = $"/data/attributes/{attributeName}"
                },
                Meta = modelError.Exception != null ? ErrorMeta.FromException(modelError.Exception) : null
            };
        }
    }
}
