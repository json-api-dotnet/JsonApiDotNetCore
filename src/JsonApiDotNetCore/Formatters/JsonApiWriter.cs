using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using JsonApiDotNetCore.Exceptions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Models.JsonApiDocuments;
using JsonApiDotNetCore.Serialization.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Formatters
{
    /// <summary>
    /// Formats the response data used  https://docs.microsoft.com/en-us/aspnet/core/web-api/advanced/formatting?view=aspnetcore-3.0.
    /// It was intended to have as little dependencies as possible in formatting layer for greater extensibility.
    /// </summary>
    public class JsonApiWriter : IJsonApiWriter
    {
        private readonly IJsonApiSerializer _serializer;
        private readonly IExceptionHandler _exceptionHandler;

        public JsonApiWriter(IJsonApiSerializer serializer, IExceptionHandler exceptionHandler)
        {
            _serializer = serializer;
            _exceptionHandler = exceptionHandler;
        }

        public async Task WriteAsync(OutputFormatterWriteContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var response = context.HttpContext.Response;
            await using var writer = context.WriterFactory(response.Body, Encoding.UTF8);
            string responseContent;

            if (_serializer == null)
            {
                responseContent = JsonConvert.SerializeObject(context.Object);
            }
            else
            {
                response.ContentType = Constants.ContentType;
                try
                {
                    responseContent = SerializeResponse(context.Object, (HttpStatusCode)response.StatusCode);
                }
                catch (Exception exception)
                {
                    var errorDocument = _exceptionHandler.HandleException(exception);
                    responseContent = _serializer.Serialize(errorDocument);

                    response.StatusCode = (int)errorDocument.GetErrorStatusCode();
                }
            }

            await writer.WriteAsync(responseContent);
            await writer.FlushAsync();
        }

        private string SerializeResponse(object contextObject, HttpStatusCode statusCode)
        {
            if (contextObject is ProblemDetails problemDetails)
            {
                throw new UnsuccessfulActionResultException(problemDetails);
            }

            if (contextObject == null && !IsSuccessStatusCode(statusCode))
            {
                throw new UnsuccessfulActionResultException(statusCode);
            }

            contextObject = WrapErrors(contextObject);

            return _serializer.Serialize(contextObject);
        }

        private static object WrapErrors(object contextObject)
        {
            if (contextObject is IEnumerable<Error> errors)
            {
                contextObject = new ErrorDocument(errors);
            }

            if (contextObject is Error error)
            {
                contextObject = new ErrorDocument(error);
            }

            return contextObject;
        }

        private bool IsSuccessStatusCode(HttpStatusCode statusCode)
        {
            return new HttpResponseMessage(statusCode).IsSuccessStatusCode;
        }
    }
}
