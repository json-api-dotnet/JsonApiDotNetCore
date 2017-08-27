using System;
using System.IO;
using System.Threading.Tasks;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Formatters
{
    public interface IJsonApiOperationsReader
    {
        Task<InputFormatterResult> ReadAsync(InputFormatterContext context);
    }

    public class JsonApiOperationsReader : IJsonApiOperationsReader
    {
        public JsonApiOperationsReader()
        {
        }

        public Task<InputFormatterResult> ReadAsync(InputFormatterContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var request = context.HttpContext.Request;
            if (request.ContentLength == 0)
                return InputFormatterResult.FailureAsync();

            return InputFormatterResult.SuccessAsync(null);
        }

        private string GetRequestBody(Stream body)
        {
            using (var reader = new StreamReader(body))
            {
                return reader.ReadToEnd();
            }
        }
    }
}