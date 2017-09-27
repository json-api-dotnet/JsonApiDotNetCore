using System;
using System.IO;
using System.Threading.Tasks;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models.Operations;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Formatters
{
    public interface IJsonApiOperationsReader
    {
        Task<InputFormatterResult> ReadAsync(InputFormatterContext context);
    }

    public class JsonApiOperationsReader : IJsonApiOperationsReader
    {
        public Task<InputFormatterResult> ReadAsync(InputFormatterContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var request = context.HttpContext.Request;
            if (request.ContentLength == null || request.ContentLength == 0)
                throw new JsonApiException(400, "Content-Length cannot be empty.");

            var body = GetRequestBody(request.Body);

            var operations = JsonConvert.DeserializeObject<OperationsDocument>(body);

            if (operations == null)
                throw new JsonApiException(400, "Failed to deserialize operations request.");

            return InputFormatterResult.SuccessAsync(operations);
        }

        private string GetRequestBody(Stream body)
        {
            using (var reader = new StreamReader(body))
                return reader.ReadToEnd();
        }
    }
}