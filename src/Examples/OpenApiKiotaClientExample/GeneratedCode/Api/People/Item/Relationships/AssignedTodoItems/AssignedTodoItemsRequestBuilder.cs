// <auto-generated/>
#nullable enable
#pragma warning disable CS8625
#pragma warning disable CS0618
using Microsoft.Kiota.Abstractions.Extensions;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions;
using OpenApiKiotaClientExample.GeneratedCode.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System;
namespace OpenApiKiotaClientExample.GeneratedCode.Api.People.Item.Relationships.AssignedTodoItems
{
    /// <summary>
    /// Builds and executes requests for operations under \api\people\{id}\relationships\assignedTodoItems
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCode("Kiota", "1.0.0")]
    public partial class AssignedTodoItemsRequestBuilder : BaseRequestBuilder
    {
        /// <summary>
        /// Instantiates a new <see cref="global::OpenApiKiotaClientExample.GeneratedCode.Api.People.Item.Relationships.AssignedTodoItems.AssignedTodoItemsRequestBuilder"/> and sets the default values.
        /// </summary>
        /// <param name="pathParameters">Path parameters for the request</param>
        /// <param name="requestAdapter">The request adapter to use to execute the requests.</param>
        public AssignedTodoItemsRequestBuilder(Dictionary<string, object> pathParameters, IRequestAdapter requestAdapter) : base(requestAdapter, "{+baseurl}/api/people/{id}/relationships/assignedTodoItems{?query*}", pathParameters)
        {
        }

        /// <summary>
        /// Instantiates a new <see cref="global::OpenApiKiotaClientExample.GeneratedCode.Api.People.Item.Relationships.AssignedTodoItems.AssignedTodoItemsRequestBuilder"/> and sets the default values.
        /// </summary>
        /// <param name="rawUrl">The raw URL to use for the request builder.</param>
        /// <param name="requestAdapter">The request adapter to use to execute the requests.</param>
        public AssignedTodoItemsRequestBuilder(string rawUrl, IRequestAdapter requestAdapter) : base(requestAdapter, "{+baseurl}/api/people/{id}/relationships/assignedTodoItems{?query*}", rawUrl)
        {
        }

        /// <summary>
        /// Removes existing todoItems from the assignedTodoItems relationship of an individual person.
        /// </summary>
        /// <param name="body">The request body</param>
        /// <param name="cancellationToken">Cancellation token to use when cancelling requests</param>
        /// <param name="requestConfiguration">Configuration for the request such as headers, query parameters, and middleware options.</param>
        /// <exception cref="global::OpenApiKiotaClientExample.GeneratedCode.Models.ErrorResponseDocument">When receiving a 400 status code</exception>
        /// <exception cref="global::OpenApiKiotaClientExample.GeneratedCode.Models.ErrorResponseDocument">When receiving a 404 status code</exception>
        /// <exception cref="global::OpenApiKiotaClientExample.GeneratedCode.Models.ErrorResponseDocument">When receiving a 409 status code</exception>
        /// <exception cref="global::OpenApiKiotaClientExample.GeneratedCode.Models.ErrorResponseDocument">When receiving a 422 status code</exception>
        public async Task DeleteAsync(global::OpenApiKiotaClientExample.GeneratedCode.Models.ToManyTodoItemInRequest body, Action<RequestConfiguration<DefaultQueryParameters>>? requestConfiguration = default, CancellationToken cancellationToken = default)
        {
            _ = body ?? throw new ArgumentNullException(nameof(body));
            var requestInfo = ToDeleteRequestInformation(body, requestConfiguration);
            var errorMapping = new Dictionary<string, ParsableFactory<IParsable>>
            {
                { "400", global::OpenApiKiotaClientExample.GeneratedCode.Models.ErrorResponseDocument.CreateFromDiscriminatorValue },
                { "404", global::OpenApiKiotaClientExample.GeneratedCode.Models.ErrorResponseDocument.CreateFromDiscriminatorValue },
                { "409", global::OpenApiKiotaClientExample.GeneratedCode.Models.ErrorResponseDocument.CreateFromDiscriminatorValue },
                { "422", global::OpenApiKiotaClientExample.GeneratedCode.Models.ErrorResponseDocument.CreateFromDiscriminatorValue },
            };
            await RequestAdapter.SendNoContentAsync(requestInfo, errorMapping, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves the related todoItem identities of an individual person&apos;s assignedTodoItems relationship.
        /// </summary>
        /// <returns>A <see cref="global::OpenApiKiotaClientExample.GeneratedCode.Models.TodoItemIdentifierCollectionResponseDocument"/></returns>
        /// <param name="cancellationToken">Cancellation token to use when cancelling requests</param>
        /// <param name="requestConfiguration">Configuration for the request such as headers, query parameters, and middleware options.</param>
        /// <exception cref="global::OpenApiKiotaClientExample.GeneratedCode.Models.ErrorResponseDocument">When receiving a 400 status code</exception>
        /// <exception cref="global::OpenApiKiotaClientExample.GeneratedCode.Models.ErrorResponseDocument">When receiving a 404 status code</exception>
        public async Task<global::OpenApiKiotaClientExample.GeneratedCode.Models.TodoItemIdentifierCollectionResponseDocument?> GetAsync(Action<RequestConfiguration<global::OpenApiKiotaClientExample.GeneratedCode.Api.People.Item.Relationships.AssignedTodoItems.AssignedTodoItemsRequestBuilder.AssignedTodoItemsRequestBuilderGetQueryParameters>>? requestConfiguration = default, CancellationToken cancellationToken = default)
        {
            var requestInfo = ToGetRequestInformation(requestConfiguration);
            var errorMapping = new Dictionary<string, ParsableFactory<IParsable>>
            {
                { "400", global::OpenApiKiotaClientExample.GeneratedCode.Models.ErrorResponseDocument.CreateFromDiscriminatorValue },
                { "404", global::OpenApiKiotaClientExample.GeneratedCode.Models.ErrorResponseDocument.CreateFromDiscriminatorValue },
            };
            return await RequestAdapter.SendAsync<global::OpenApiKiotaClientExample.GeneratedCode.Models.TodoItemIdentifierCollectionResponseDocument>(requestInfo, global::OpenApiKiotaClientExample.GeneratedCode.Models.TodoItemIdentifierCollectionResponseDocument.CreateFromDiscriminatorValue, errorMapping, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Compare the returned ETag HTTP header with an earlier one to determine if the response has changed since it was fetched.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to use when cancelling requests</param>
        /// <param name="requestConfiguration">Configuration for the request such as headers, query parameters, and middleware options.</param>
        public async Task HeadAsync(Action<RequestConfiguration<global::OpenApiKiotaClientExample.GeneratedCode.Api.People.Item.Relationships.AssignedTodoItems.AssignedTodoItemsRequestBuilder.AssignedTodoItemsRequestBuilderHeadQueryParameters>>? requestConfiguration = default, CancellationToken cancellationToken = default)
        {
            var requestInfo = ToHeadRequestInformation(requestConfiguration);
            await RequestAdapter.SendNoContentAsync(requestInfo, default, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Assigns existing todoItems to the assignedTodoItems relationship of an individual person.
        /// </summary>
        /// <param name="body">The request body</param>
        /// <param name="cancellationToken">Cancellation token to use when cancelling requests</param>
        /// <param name="requestConfiguration">Configuration for the request such as headers, query parameters, and middleware options.</param>
        /// <exception cref="global::OpenApiKiotaClientExample.GeneratedCode.Models.ErrorResponseDocument">When receiving a 400 status code</exception>
        /// <exception cref="global::OpenApiKiotaClientExample.GeneratedCode.Models.ErrorResponseDocument">When receiving a 404 status code</exception>
        /// <exception cref="global::OpenApiKiotaClientExample.GeneratedCode.Models.ErrorResponseDocument">When receiving a 409 status code</exception>
        /// <exception cref="global::OpenApiKiotaClientExample.GeneratedCode.Models.ErrorResponseDocument">When receiving a 422 status code</exception>
        public async Task PatchAsync(global::OpenApiKiotaClientExample.GeneratedCode.Models.ToManyTodoItemInRequest body, Action<RequestConfiguration<DefaultQueryParameters>>? requestConfiguration = default, CancellationToken cancellationToken = default)
        {
            _ = body ?? throw new ArgumentNullException(nameof(body));
            var requestInfo = ToPatchRequestInformation(body, requestConfiguration);
            var errorMapping = new Dictionary<string, ParsableFactory<IParsable>>
            {
                { "400", global::OpenApiKiotaClientExample.GeneratedCode.Models.ErrorResponseDocument.CreateFromDiscriminatorValue },
                { "404", global::OpenApiKiotaClientExample.GeneratedCode.Models.ErrorResponseDocument.CreateFromDiscriminatorValue },
                { "409", global::OpenApiKiotaClientExample.GeneratedCode.Models.ErrorResponseDocument.CreateFromDiscriminatorValue },
                { "422", global::OpenApiKiotaClientExample.GeneratedCode.Models.ErrorResponseDocument.CreateFromDiscriminatorValue },
            };
            await RequestAdapter.SendNoContentAsync(requestInfo, errorMapping, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Adds existing todoItems to the assignedTodoItems relationship of an individual person.
        /// </summary>
        /// <param name="body">The request body</param>
        /// <param name="cancellationToken">Cancellation token to use when cancelling requests</param>
        /// <param name="requestConfiguration">Configuration for the request such as headers, query parameters, and middleware options.</param>
        /// <exception cref="global::OpenApiKiotaClientExample.GeneratedCode.Models.ErrorResponseDocument">When receiving a 400 status code</exception>
        /// <exception cref="global::OpenApiKiotaClientExample.GeneratedCode.Models.ErrorResponseDocument">When receiving a 404 status code</exception>
        /// <exception cref="global::OpenApiKiotaClientExample.GeneratedCode.Models.ErrorResponseDocument">When receiving a 409 status code</exception>
        /// <exception cref="global::OpenApiKiotaClientExample.GeneratedCode.Models.ErrorResponseDocument">When receiving a 422 status code</exception>
        public async Task PostAsync(global::OpenApiKiotaClientExample.GeneratedCode.Models.ToManyTodoItemInRequest body, Action<RequestConfiguration<DefaultQueryParameters>>? requestConfiguration = default, CancellationToken cancellationToken = default)
        {
            _ = body ?? throw new ArgumentNullException(nameof(body));
            var requestInfo = ToPostRequestInformation(body, requestConfiguration);
            var errorMapping = new Dictionary<string, ParsableFactory<IParsable>>
            {
                { "400", global::OpenApiKiotaClientExample.GeneratedCode.Models.ErrorResponseDocument.CreateFromDiscriminatorValue },
                { "404", global::OpenApiKiotaClientExample.GeneratedCode.Models.ErrorResponseDocument.CreateFromDiscriminatorValue },
                { "409", global::OpenApiKiotaClientExample.GeneratedCode.Models.ErrorResponseDocument.CreateFromDiscriminatorValue },
                { "422", global::OpenApiKiotaClientExample.GeneratedCode.Models.ErrorResponseDocument.CreateFromDiscriminatorValue },
            };
            await RequestAdapter.SendNoContentAsync(requestInfo, errorMapping, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Removes existing todoItems from the assignedTodoItems relationship of an individual person.
        /// </summary>
        /// <returns>A <see cref="RequestInformation"/></returns>
        /// <param name="body">The request body</param>
        /// <param name="requestConfiguration">Configuration for the request such as headers, query parameters, and middleware options.</param>
        public RequestInformation ToDeleteRequestInformation(global::OpenApiKiotaClientExample.GeneratedCode.Models.ToManyTodoItemInRequest body, Action<RequestConfiguration<DefaultQueryParameters>>? requestConfiguration = default)
        {
            _ = body ?? throw new ArgumentNullException(nameof(body));
            var requestInfo = new RequestInformation(Method.DELETE, UrlTemplate, PathParameters);
            requestInfo.Configure(requestConfiguration);
            requestInfo.Headers.TryAdd("Accept", "application/vnd.api+json;ext=openapi");
            requestInfo.SetContentFromParsable(RequestAdapter, "application/vnd.api+json;ext=openapi", body);
            return requestInfo;
        }

        /// <summary>
        /// Retrieves the related todoItem identities of an individual person&apos;s assignedTodoItems relationship.
        /// </summary>
        /// <returns>A <see cref="RequestInformation"/></returns>
        /// <param name="requestConfiguration">Configuration for the request such as headers, query parameters, and middleware options.</param>
        public RequestInformation ToGetRequestInformation(Action<RequestConfiguration<global::OpenApiKiotaClientExample.GeneratedCode.Api.People.Item.Relationships.AssignedTodoItems.AssignedTodoItemsRequestBuilder.AssignedTodoItemsRequestBuilderGetQueryParameters>>? requestConfiguration = default)
        {
            var requestInfo = new RequestInformation(Method.GET, UrlTemplate, PathParameters);
            requestInfo.Configure(requestConfiguration);
            requestInfo.Headers.TryAdd("Accept", "application/vnd.api+json;ext=openapi");
            return requestInfo;
        }

        /// <summary>
        /// Compare the returned ETag HTTP header with an earlier one to determine if the response has changed since it was fetched.
        /// </summary>
        /// <returns>A <see cref="RequestInformation"/></returns>
        /// <param name="requestConfiguration">Configuration for the request such as headers, query parameters, and middleware options.</param>
        public RequestInformation ToHeadRequestInformation(Action<RequestConfiguration<global::OpenApiKiotaClientExample.GeneratedCode.Api.People.Item.Relationships.AssignedTodoItems.AssignedTodoItemsRequestBuilder.AssignedTodoItemsRequestBuilderHeadQueryParameters>>? requestConfiguration = default)
        {
            var requestInfo = new RequestInformation(Method.HEAD, UrlTemplate, PathParameters);
            requestInfo.Configure(requestConfiguration);
            return requestInfo;
        }

        /// <summary>
        /// Assigns existing todoItems to the assignedTodoItems relationship of an individual person.
        /// </summary>
        /// <returns>A <see cref="RequestInformation"/></returns>
        /// <param name="body">The request body</param>
        /// <param name="requestConfiguration">Configuration for the request such as headers, query parameters, and middleware options.</param>
        public RequestInformation ToPatchRequestInformation(global::OpenApiKiotaClientExample.GeneratedCode.Models.ToManyTodoItemInRequest body, Action<RequestConfiguration<DefaultQueryParameters>>? requestConfiguration = default)
        {
            _ = body ?? throw new ArgumentNullException(nameof(body));
            var requestInfo = new RequestInformation(Method.PATCH, UrlTemplate, PathParameters);
            requestInfo.Configure(requestConfiguration);
            requestInfo.Headers.TryAdd("Accept", "application/vnd.api+json;ext=openapi");
            requestInfo.SetContentFromParsable(RequestAdapter, "application/vnd.api+json;ext=openapi", body);
            return requestInfo;
        }

        /// <summary>
        /// Adds existing todoItems to the assignedTodoItems relationship of an individual person.
        /// </summary>
        /// <returns>A <see cref="RequestInformation"/></returns>
        /// <param name="body">The request body</param>
        /// <param name="requestConfiguration">Configuration for the request such as headers, query parameters, and middleware options.</param>
        public RequestInformation ToPostRequestInformation(global::OpenApiKiotaClientExample.GeneratedCode.Models.ToManyTodoItemInRequest body, Action<RequestConfiguration<DefaultQueryParameters>>? requestConfiguration = default)
        {
            _ = body ?? throw new ArgumentNullException(nameof(body));
            var requestInfo = new RequestInformation(Method.POST, UrlTemplate, PathParameters);
            requestInfo.Configure(requestConfiguration);
            requestInfo.Headers.TryAdd("Accept", "application/vnd.api+json;ext=openapi");
            requestInfo.SetContentFromParsable(RequestAdapter, "application/vnd.api+json;ext=openapi", body);
            return requestInfo;
        }

        /// <summary>
        /// Returns a request builder with the provided arbitrary URL. Using this method means any other path or query parameters are ignored.
        /// </summary>
        /// <returns>A <see cref="global::OpenApiKiotaClientExample.GeneratedCode.Api.People.Item.Relationships.AssignedTodoItems.AssignedTodoItemsRequestBuilder"/></returns>
        /// <param name="rawUrl">The raw URL to use for the request builder.</param>
        public global::OpenApiKiotaClientExample.GeneratedCode.Api.People.Item.Relationships.AssignedTodoItems.AssignedTodoItemsRequestBuilder WithUrl(string rawUrl)
        {
            return new global::OpenApiKiotaClientExample.GeneratedCode.Api.People.Item.Relationships.AssignedTodoItems.AssignedTodoItemsRequestBuilder(rawUrl, RequestAdapter);
        }

        /// <summary>
        /// Retrieves the related todoItem identities of an individual person&apos;s assignedTodoItems relationship.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCode("Kiota", "1.0.0")]
        public partial class AssignedTodoItemsRequestBuilderGetQueryParameters 
        {
            /// <summary>For syntax, see the documentation for the [`filter`](https://www.jsonapi.net/usage/reading/filtering.html)/[`sort`](https://www.jsonapi.net/usage/reading/sorting.html)/[`page`](https://www.jsonapi.net/usage/reading/pagination.html)/[`fields`](https://www.jsonapi.net/usage/reading/sparse-fieldset-selection.html) query string parameters.</summary>
            [QueryParameter("query")]
            public string? Query { get; set; }
        }

        /// <summary>
        /// Compare the returned ETag HTTP header with an earlier one to determine if the response has changed since it was fetched.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCode("Kiota", "1.0.0")]
        public partial class AssignedTodoItemsRequestBuilderHeadQueryParameters 
        {
            /// <summary>For syntax, see the documentation for the [`filter`](https://www.jsonapi.net/usage/reading/filtering.html)/[`sort`](https://www.jsonapi.net/usage/reading/sorting.html)/[`page`](https://www.jsonapi.net/usage/reading/pagination.html)/[`fields`](https://www.jsonapi.net/usage/reading/sparse-fieldset-selection.html) query string parameters.</summary>
            [QueryParameter("query")]
            public string? Query { get; set; }
        }
    }
}
#pragma warning restore CS0618
