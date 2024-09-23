// <auto-generated/>
using Microsoft.Kiota.Abstractions;
using OpenApiKiotaClientExample.GeneratedCode.Api.Operations;
using OpenApiKiotaClientExample.GeneratedCode.Api.People;
using OpenApiKiotaClientExample.GeneratedCode.Api.Tags;
using OpenApiKiotaClientExample.GeneratedCode.Api.TodoItems;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;
namespace OpenApiKiotaClientExample.GeneratedCode.Api
{
    /// <summary>
    /// Builds and executes requests for operations under \api
    /// </summary>
    public class ApiRequestBuilder : BaseRequestBuilder
    {
        /// <summary>The operations property</summary>
        public OpenApiKiotaClientExample.GeneratedCode.Api.Operations.OperationsRequestBuilder Operations
        {
            get => new OpenApiKiotaClientExample.GeneratedCode.Api.Operations.OperationsRequestBuilder(PathParameters, RequestAdapter);
        }
        /// <summary>The people property</summary>
        public OpenApiKiotaClientExample.GeneratedCode.Api.People.PeopleRequestBuilder People
        {
            get => new OpenApiKiotaClientExample.GeneratedCode.Api.People.PeopleRequestBuilder(PathParameters, RequestAdapter);
        }
        /// <summary>The tags property</summary>
        public OpenApiKiotaClientExample.GeneratedCode.Api.Tags.TagsRequestBuilder Tags
        {
            get => new OpenApiKiotaClientExample.GeneratedCode.Api.Tags.TagsRequestBuilder(PathParameters, RequestAdapter);
        }
        /// <summary>The todoItems property</summary>
        public OpenApiKiotaClientExample.GeneratedCode.Api.TodoItems.TodoItemsRequestBuilder TodoItems
        {
            get => new OpenApiKiotaClientExample.GeneratedCode.Api.TodoItems.TodoItemsRequestBuilder(PathParameters, RequestAdapter);
        }
        /// <summary>
        /// Instantiates a new <see cref="OpenApiKiotaClientExample.GeneratedCode.Api.ApiRequestBuilder"/> and sets the default values.
        /// </summary>
        /// <param name="pathParameters">Path parameters for the request</param>
        /// <param name="requestAdapter">The request adapter to use to execute the requests.</param>
        public ApiRequestBuilder(Dictionary<string, object> pathParameters, IRequestAdapter requestAdapter) : base(requestAdapter, "{+baseurl}/api", pathParameters)
        {
        }
        /// <summary>
        /// Instantiates a new <see cref="OpenApiKiotaClientExample.GeneratedCode.Api.ApiRequestBuilder"/> and sets the default values.
        /// </summary>
        /// <param name="rawUrl">The raw URL to use for the request builder.</param>
        /// <param name="requestAdapter">The request adapter to use to execute the requests.</param>
        public ApiRequestBuilder(string rawUrl, IRequestAdapter requestAdapter) : base(requestAdapter, "{+baseurl}/api", rawUrl)
        {
        }
    }
}