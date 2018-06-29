using JsonApiDotNetCore.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore.Internal;

namespace JsonApiDotNetCore.Extensions
{
    public static class ModelStateExtensions
    {
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
    }
}
