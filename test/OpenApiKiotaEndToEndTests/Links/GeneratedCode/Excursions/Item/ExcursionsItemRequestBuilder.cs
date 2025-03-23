// <auto-generated/>
#nullable enable
#pragma warning disable CS8625
#pragma warning disable CS0618
using Microsoft.Kiota.Abstractions.Extensions;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions;
using OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System;
namespace OpenApiKiotaEndToEndTests.Links.GeneratedCode.Excursions.Item
{
    /// <summary>
    /// Builds and executes requests for operations under \excursions\{id}
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCode("Kiota", "1.0.0")]
    public partial class ExcursionsItemRequestBuilder : BaseRequestBuilder
    {
        /// <summary>
        /// Instantiates a new <see cref="global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Excursions.Item.ExcursionsItemRequestBuilder"/> and sets the default values.
        /// </summary>
        /// <param name="pathParameters">Path parameters for the request</param>
        /// <param name="requestAdapter">The request adapter to use to execute the requests.</param>
        public ExcursionsItemRequestBuilder(Dictionary<string, object> pathParameters, IRequestAdapter requestAdapter) : base(requestAdapter, "{+baseurl}/excursions/{id}{?query*}", pathParameters)
        {
        }

        /// <summary>
        /// Instantiates a new <see cref="global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Excursions.Item.ExcursionsItemRequestBuilder"/> and sets the default values.
        /// </summary>
        /// <param name="rawUrl">The raw URL to use for the request builder.</param>
        /// <param name="requestAdapter">The request adapter to use to execute the requests.</param>
        public ExcursionsItemRequestBuilder(string rawUrl, IRequestAdapter requestAdapter) : base(requestAdapter, "{+baseurl}/excursions/{id}{?query*}", rawUrl)
        {
        }

        /// <summary>
        /// Deletes an existing excursion by its identifier.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to use when cancelling requests</param>
        /// <param name="requestConfiguration">Configuration for the request such as headers, query parameters, and middleware options.</param>
        /// <exception cref="global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.ErrorResponseDocument">When receiving a 404 status code</exception>
        public async Task DeleteAsync(Action<RequestConfiguration<DefaultQueryParameters>>? requestConfiguration = default, CancellationToken cancellationToken = default)
        {
            var requestInfo = ToDeleteRequestInformation(requestConfiguration);
            var errorMapping = new Dictionary<string, ParsableFactory<IParsable>>
            {
                { "404", global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.ErrorResponseDocument.CreateFromDiscriminatorValue },
            };
            await RequestAdapter.SendNoContentAsync(requestInfo, errorMapping, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves an individual excursion by its identifier.
        /// </summary>
        /// <returns>A <see cref="global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.PrimaryExcursionResponseDocument"/></returns>
        /// <param name="cancellationToken">Cancellation token to use when cancelling requests</param>
        /// <param name="requestConfiguration">Configuration for the request such as headers, query parameters, and middleware options.</param>
        /// <exception cref="global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.ErrorResponseDocument">When receiving a 400 status code</exception>
        /// <exception cref="global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.ErrorResponseDocument">When receiving a 404 status code</exception>
        public async Task<global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.PrimaryExcursionResponseDocument?> GetAsync(Action<RequestConfiguration<global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Excursions.Item.ExcursionsItemRequestBuilder.ExcursionsItemRequestBuilderGetQueryParameters>>? requestConfiguration = default, CancellationToken cancellationToken = default)
        {
            var requestInfo = ToGetRequestInformation(requestConfiguration);
            var errorMapping = new Dictionary<string, ParsableFactory<IParsable>>
            {
                { "400", global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.ErrorResponseDocument.CreateFromDiscriminatorValue },
                { "404", global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.ErrorResponseDocument.CreateFromDiscriminatorValue },
            };
            return await RequestAdapter.SendAsync<global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.PrimaryExcursionResponseDocument>(requestInfo, global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.PrimaryExcursionResponseDocument.CreateFromDiscriminatorValue, errorMapping, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Compare the returned ETag HTTP header with an earlier one to determine if the response has changed since it was fetched.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to use when cancelling requests</param>
        /// <param name="requestConfiguration">Configuration for the request such as headers, query parameters, and middleware options.</param>
        public async Task HeadAsync(Action<RequestConfiguration<global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Excursions.Item.ExcursionsItemRequestBuilder.ExcursionsItemRequestBuilderHeadQueryParameters>>? requestConfiguration = default, CancellationToken cancellationToken = default)
        {
            var requestInfo = ToHeadRequestInformation(requestConfiguration);
            await RequestAdapter.SendNoContentAsync(requestInfo, default, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Updates an existing excursion.
        /// </summary>
        /// <returns>A <see cref="global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.PrimaryExcursionResponseDocument"/></returns>
        /// <param name="body">The request body</param>
        /// <param name="cancellationToken">Cancellation token to use when cancelling requests</param>
        /// <param name="requestConfiguration">Configuration for the request such as headers, query parameters, and middleware options.</param>
        /// <exception cref="global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.ErrorResponseDocument">When receiving a 400 status code</exception>
        /// <exception cref="global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.ErrorResponseDocument">When receiving a 404 status code</exception>
        /// <exception cref="global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.ErrorResponseDocument">When receiving a 409 status code</exception>
        /// <exception cref="global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.ErrorResponseDocument">When receiving a 422 status code</exception>
        public async Task<global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.PrimaryExcursionResponseDocument?> PatchAsync(global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.UpdateExcursionRequestDocument body, Action<RequestConfiguration<global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Excursions.Item.ExcursionsItemRequestBuilder.ExcursionsItemRequestBuilderPatchQueryParameters>>? requestConfiguration = default, CancellationToken cancellationToken = default)
        {
            _ = body ?? throw new ArgumentNullException(nameof(body));
            var requestInfo = ToPatchRequestInformation(body, requestConfiguration);
            var errorMapping = new Dictionary<string, ParsableFactory<IParsable>>
            {
                { "400", global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.ErrorResponseDocument.CreateFromDiscriminatorValue },
                { "404", global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.ErrorResponseDocument.CreateFromDiscriminatorValue },
                { "409", global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.ErrorResponseDocument.CreateFromDiscriminatorValue },
                { "422", global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.ErrorResponseDocument.CreateFromDiscriminatorValue },
            };
            return await RequestAdapter.SendAsync<global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.PrimaryExcursionResponseDocument>(requestInfo, global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.PrimaryExcursionResponseDocument.CreateFromDiscriminatorValue, errorMapping, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes an existing excursion by its identifier.
        /// </summary>
        /// <returns>A <see cref="RequestInformation"/></returns>
        /// <param name="requestConfiguration">Configuration for the request such as headers, query parameters, and middleware options.</param>
        public RequestInformation ToDeleteRequestInformation(Action<RequestConfiguration<DefaultQueryParameters>>? requestConfiguration = default)
        {
            var requestInfo = new RequestInformation(Method.DELETE, UrlTemplate, PathParameters);
            requestInfo.Configure(requestConfiguration);
            requestInfo.Headers.TryAdd("Accept", "application/vnd.api+json");
            return requestInfo;
        }

        /// <summary>
        /// Retrieves an individual excursion by its identifier.
        /// </summary>
        /// <returns>A <see cref="RequestInformation"/></returns>
        /// <param name="requestConfiguration">Configuration for the request such as headers, query parameters, and middleware options.</param>
        public RequestInformation ToGetRequestInformation(Action<RequestConfiguration<global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Excursions.Item.ExcursionsItemRequestBuilder.ExcursionsItemRequestBuilderGetQueryParameters>>? requestConfiguration = default)
        {
            var requestInfo = new RequestInformation(Method.GET, UrlTemplate, PathParameters);
            requestInfo.Configure(requestConfiguration);
            requestInfo.Headers.TryAdd("Accept", "application/vnd.api+json");
            return requestInfo;
        }

        /// <summary>
        /// Compare the returned ETag HTTP header with an earlier one to determine if the response has changed since it was fetched.
        /// </summary>
        /// <returns>A <see cref="RequestInformation"/></returns>
        /// <param name="requestConfiguration">Configuration for the request such as headers, query parameters, and middleware options.</param>
        public RequestInformation ToHeadRequestInformation(Action<RequestConfiguration<global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Excursions.Item.ExcursionsItemRequestBuilder.ExcursionsItemRequestBuilderHeadQueryParameters>>? requestConfiguration = default)
        {
            var requestInfo = new RequestInformation(Method.HEAD, UrlTemplate, PathParameters);
            requestInfo.Configure(requestConfiguration);
            return requestInfo;
        }

        /// <summary>
        /// Updates an existing excursion.
        /// </summary>
        /// <returns>A <see cref="RequestInformation"/></returns>
        /// <param name="body">The request body</param>
        /// <param name="requestConfiguration">Configuration for the request such as headers, query parameters, and middleware options.</param>
        public RequestInformation ToPatchRequestInformation(global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.UpdateExcursionRequestDocument body, Action<RequestConfiguration<global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Excursions.Item.ExcursionsItemRequestBuilder.ExcursionsItemRequestBuilderPatchQueryParameters>>? requestConfiguration = default)
        {
            _ = body ?? throw new ArgumentNullException(nameof(body));
            var requestInfo = new RequestInformation(Method.PATCH, UrlTemplate, PathParameters);
            requestInfo.Configure(requestConfiguration);
            requestInfo.Headers.TryAdd("Accept", "application/vnd.api+json");
            requestInfo.SetContentFromParsable(RequestAdapter, "application/vnd.api+json", body);
            return requestInfo;
        }

        /// <summary>
        /// Returns a request builder with the provided arbitrary URL. Using this method means any other path or query parameters are ignored.
        /// </summary>
        /// <returns>A <see cref="global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Excursions.Item.ExcursionsItemRequestBuilder"/></returns>
        /// <param name="rawUrl">The raw URL to use for the request builder.</param>
        public global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Excursions.Item.ExcursionsItemRequestBuilder WithUrl(string rawUrl)
        {
            return new global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Excursions.Item.ExcursionsItemRequestBuilder(rawUrl, RequestAdapter);
        }

        /// <summary>
        /// Retrieves an individual excursion by its identifier.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCode("Kiota", "1.0.0")]
        public partial class ExcursionsItemRequestBuilderGetQueryParameters 
        {
            /// <summary>For syntax, see the documentation for the [`include`](https://www.jsonapi.net/usage/reading/including-relationships.html)/[`filter`](https://www.jsonapi.net/usage/reading/filtering.html)/[`sort`](https://www.jsonapi.net/usage/reading/sorting.html)/[`page`](https://www.jsonapi.net/usage/reading/pagination.html)/[`fields`](https://www.jsonapi.net/usage/reading/sparse-fieldset-selection.html) query string parameters.</summary>
            [QueryParameter("query")]
            public string? Query { get; set; }
        }

        /// <summary>
        /// Compare the returned ETag HTTP header with an earlier one to determine if the response has changed since it was fetched.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCode("Kiota", "1.0.0")]
        public partial class ExcursionsItemRequestBuilderHeadQueryParameters 
        {
            /// <summary>For syntax, see the documentation for the [`include`](https://www.jsonapi.net/usage/reading/including-relationships.html)/[`filter`](https://www.jsonapi.net/usage/reading/filtering.html)/[`sort`](https://www.jsonapi.net/usage/reading/sorting.html)/[`page`](https://www.jsonapi.net/usage/reading/pagination.html)/[`fields`](https://www.jsonapi.net/usage/reading/sparse-fieldset-selection.html) query string parameters.</summary>
            [QueryParameter("query")]
            public string? Query { get; set; }
        }

        /// <summary>
        /// Updates an existing excursion.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCode("Kiota", "1.0.0")]
        public partial class ExcursionsItemRequestBuilderPatchQueryParameters 
        {
            /// <summary>For syntax, see the documentation for the [`include`](https://www.jsonapi.net/usage/reading/including-relationships.html)/[`filter`](https://www.jsonapi.net/usage/reading/filtering.html)/[`sort`](https://www.jsonapi.net/usage/reading/sorting.html)/[`page`](https://www.jsonapi.net/usage/reading/pagination.html)/[`fields`](https://www.jsonapi.net/usage/reading/sparse-fieldset-selection.html) query string parameters.</summary>
            [QueryParameter("query")]
            public string? Query { get; set; }
        }
    }
}
#pragma warning restore CS0618
