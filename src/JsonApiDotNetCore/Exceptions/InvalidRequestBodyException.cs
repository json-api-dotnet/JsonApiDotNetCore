using System;
using System.Net;
using JsonApiDotNetCore.Models.JsonApiDocuments;

namespace JsonApiDotNetCore.Exceptions
{
    /// <summary>
    /// The error that is thrown when deserializing the request body fails.
    /// </summary>
    public sealed class InvalidRequestBodyException : JsonApiException
    {
        private readonly string _details;
        private string _requestBody;

        public InvalidRequestBodyException(string reason, string details, string requestBody, Exception innerException = null)
            : base(new Error(HttpStatusCode.UnprocessableEntity)
            {
                Title = reason != null
                    ? "Failed to deserialize request body: " + reason
                    : "Failed to deserialize request body.",
            }, innerException)
        {
            _details = details;
            _requestBody = requestBody;

            UpdateErrorDetail();
        }

        private void UpdateErrorDetail()
        {
            string text = _details ?? InnerException?.Message;

            if (_requestBody != null)
            {
                if (text != null)
                {
                    text += " - ";
                }

                text += "Request body: <<" + _requestBody + ">>";
            }

            Error.Detail = text;
        }

        public void SetRequestBody(string requestBody)
        {
            _requestBody = requestBody;
            UpdateErrorDetail();
        }
    }
}
