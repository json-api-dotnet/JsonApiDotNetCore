// <auto-generated/>
#nullable enable
#pragma warning disable CS8625
#pragma warning disable CS0618
using Microsoft.Kiota.Abstractions.Extensions;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions;
using OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Mansions.Item.Relationships;
using OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Mansions.Item.Rooms;
using OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Mansions.Item.Staff;
using OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System;
namespace OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Mansions.Item
{
    /// <summary>
    /// Builds and executes requests for operations under \mansions\{id}
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCode("Kiota", "1.0.0")]
    public partial class MansionsItemRequestBuilder : BaseRequestBuilder
    {
        /// <summary>The relationships property</summary>
        public global::OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Mansions.Item.Relationships.RelationshipsRequestBuilder Relationships
        {
            get => new global::OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Mansions.Item.Relationships.RelationshipsRequestBuilder(PathParameters, RequestAdapter);
        }

        /// <summary>The rooms property</summary>
        public global::OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Mansions.Item.Rooms.RoomsRequestBuilder Rooms
        {
            get => new global::OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Mansions.Item.Rooms.RoomsRequestBuilder(PathParameters, RequestAdapter);
        }

        /// <summary>The staff property</summary>
        public global::OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Mansions.Item.Staff.StaffRequestBuilder Staff
        {
            get => new global::OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Mansions.Item.Staff.StaffRequestBuilder(PathParameters, RequestAdapter);
        }

        /// <summary>
        /// Instantiates a new <see cref="global::OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Mansions.Item.MansionsItemRequestBuilder"/> and sets the default values.
        /// </summary>
        /// <param name="pathParameters">Path parameters for the request</param>
        /// <param name="requestAdapter">The request adapter to use to execute the requests.</param>
        public MansionsItemRequestBuilder(Dictionary<string, object> pathParameters, IRequestAdapter requestAdapter) : base(requestAdapter, "{+baseurl}/mansions/{id}{?query*}", pathParameters)
        {
        }

        /// <summary>
        /// Instantiates a new <see cref="global::OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Mansions.Item.MansionsItemRequestBuilder"/> and sets the default values.
        /// </summary>
        /// <param name="rawUrl">The raw URL to use for the request builder.</param>
        /// <param name="requestAdapter">The request adapter to use to execute the requests.</param>
        public MansionsItemRequestBuilder(string rawUrl, IRequestAdapter requestAdapter) : base(requestAdapter, "{+baseurl}/mansions/{id}{?query*}", rawUrl)
        {
        }

        /// <summary>
        /// Deletes an existing mansion by its identifier.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to use when cancelling requests</param>
        /// <param name="requestConfiguration">Configuration for the request such as headers, query parameters, and middleware options.</param>
        /// <exception cref="global::OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Models.ErrorResponseDocument">When receiving a 404 status code</exception>
        public async Task DeleteAsync(Action<RequestConfiguration<DefaultQueryParameters>>? requestConfiguration = default, CancellationToken cancellationToken = default)
        {
            var requestInfo = ToDeleteRequestInformation(requestConfiguration);
            var errorMapping = new Dictionary<string, ParsableFactory<IParsable>>
            {
                { "404", global::OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Models.ErrorResponseDocument.CreateFromDiscriminatorValue },
            };
            await RequestAdapter.SendNoContentAsync(requestInfo, errorMapping, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieves an individual mansion by its identifier.
        /// </summary>
        /// <returns>A <see cref="global::OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Models.PrimaryMansionResponseDocument"/></returns>
        /// <param name="cancellationToken">Cancellation token to use when cancelling requests</param>
        /// <param name="requestConfiguration">Configuration for the request such as headers, query parameters, and middleware options.</param>
        /// <exception cref="global::OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Models.ErrorResponseDocument">When receiving a 400 status code</exception>
        /// <exception cref="global::OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Models.ErrorResponseDocument">When receiving a 404 status code</exception>
        public async Task<global::OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Models.PrimaryMansionResponseDocument?> GetAsync(Action<RequestConfiguration<global::OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Mansions.Item.MansionsItemRequestBuilder.MansionsItemRequestBuilderGetQueryParameters>>? requestConfiguration = default, CancellationToken cancellationToken = default)
        {
            var requestInfo = ToGetRequestInformation(requestConfiguration);
            var errorMapping = new Dictionary<string, ParsableFactory<IParsable>>
            {
                { "400", global::OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Models.ErrorResponseDocument.CreateFromDiscriminatorValue },
                { "404", global::OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Models.ErrorResponseDocument.CreateFromDiscriminatorValue },
            };
            return await RequestAdapter.SendAsync<global::OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Models.PrimaryMansionResponseDocument>(requestInfo, global::OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Models.PrimaryMansionResponseDocument.CreateFromDiscriminatorValue, errorMapping, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Compare the returned ETag HTTP header with an earlier one to determine if the response has changed since it was fetched.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to use when cancelling requests</param>
        /// <param name="requestConfiguration">Configuration for the request such as headers, query parameters, and middleware options.</param>
        public async Task HeadAsync(Action<RequestConfiguration<global::OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Mansions.Item.MansionsItemRequestBuilder.MansionsItemRequestBuilderHeadQueryParameters>>? requestConfiguration = default, CancellationToken cancellationToken = default)
        {
            var requestInfo = ToHeadRequestInformation(requestConfiguration);
            await RequestAdapter.SendNoContentAsync(requestInfo, default, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Updates an existing mansion.
        /// </summary>
        /// <returns>A <see cref="global::OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Models.PrimaryMansionResponseDocument"/></returns>
        /// <param name="body">The request body</param>
        /// <param name="cancellationToken">Cancellation token to use when cancelling requests</param>
        /// <param name="requestConfiguration">Configuration for the request such as headers, query parameters, and middleware options.</param>
        /// <exception cref="global::OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Models.ErrorResponseDocument">When receiving a 400 status code</exception>
        /// <exception cref="global::OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Models.ErrorResponseDocument">When receiving a 404 status code</exception>
        /// <exception cref="global::OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Models.ErrorResponseDocument">When receiving a 409 status code</exception>
        /// <exception cref="global::OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Models.ErrorResponseDocument">When receiving a 422 status code</exception>
        public async Task<global::OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Models.PrimaryMansionResponseDocument?> PatchAsync(global::OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Models.UpdateMansionRequestDocument body, Action<RequestConfiguration<global::OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Mansions.Item.MansionsItemRequestBuilder.MansionsItemRequestBuilderPatchQueryParameters>>? requestConfiguration = default, CancellationToken cancellationToken = default)
        {
            _ = body ?? throw new ArgumentNullException(nameof(body));
            var requestInfo = ToPatchRequestInformation(body, requestConfiguration);
            var errorMapping = new Dictionary<string, ParsableFactory<IParsable>>
            {
                { "400", global::OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Models.ErrorResponseDocument.CreateFromDiscriminatorValue },
                { "404", global::OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Models.ErrorResponseDocument.CreateFromDiscriminatorValue },
                { "409", global::OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Models.ErrorResponseDocument.CreateFromDiscriminatorValue },
                { "422", global::OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Models.ErrorResponseDocument.CreateFromDiscriminatorValue },
            };
            return await RequestAdapter.SendAsync<global::OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Models.PrimaryMansionResponseDocument>(requestInfo, global::OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Models.PrimaryMansionResponseDocument.CreateFromDiscriminatorValue, errorMapping, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes an existing mansion by its identifier.
        /// </summary>
        /// <returns>A <see cref="RequestInformation"/></returns>
        /// <param name="requestConfiguration">Configuration for the request such as headers, query parameters, and middleware options.</param>
        public RequestInformation ToDeleteRequestInformation(Action<RequestConfiguration<DefaultQueryParameters>>? requestConfiguration = default)
        {
            var requestInfo = new RequestInformation(Method.DELETE, UrlTemplate, PathParameters);
            requestInfo.Configure(requestConfiguration);
            requestInfo.Headers.TryAdd("Accept", "application/vnd.api+json;ext=openapi");
            return requestInfo;
        }

        /// <summary>
        /// Retrieves an individual mansion by its identifier.
        /// </summary>
        /// <returns>A <see cref="RequestInformation"/></returns>
        /// <param name="requestConfiguration">Configuration for the request such as headers, query parameters, and middleware options.</param>
        public RequestInformation ToGetRequestInformation(Action<RequestConfiguration<global::OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Mansions.Item.MansionsItemRequestBuilder.MansionsItemRequestBuilderGetQueryParameters>>? requestConfiguration = default)
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
        public RequestInformation ToHeadRequestInformation(Action<RequestConfiguration<global::OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Mansions.Item.MansionsItemRequestBuilder.MansionsItemRequestBuilderHeadQueryParameters>>? requestConfiguration = default)
        {
            var requestInfo = new RequestInformation(Method.HEAD, UrlTemplate, PathParameters);
            requestInfo.Configure(requestConfiguration);
            return requestInfo;
        }

        /// <summary>
        /// Updates an existing mansion.
        /// </summary>
        /// <returns>A <see cref="RequestInformation"/></returns>
        /// <param name="body">The request body</param>
        /// <param name="requestConfiguration">Configuration for the request such as headers, query parameters, and middleware options.</param>
        public RequestInformation ToPatchRequestInformation(global::OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Models.UpdateMansionRequestDocument body, Action<RequestConfiguration<global::OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Mansions.Item.MansionsItemRequestBuilder.MansionsItemRequestBuilderPatchQueryParameters>>? requestConfiguration = default)
        {
            _ = body ?? throw new ArgumentNullException(nameof(body));
            var requestInfo = new RequestInformation(Method.PATCH, UrlTemplate, PathParameters);
            requestInfo.Configure(requestConfiguration);
            requestInfo.Headers.TryAdd("Accept", "application/vnd.api+json;ext=openapi");
            requestInfo.SetContentFromParsable(RequestAdapter, "application/vnd.api+json;ext=openapi", body);
            return requestInfo;
        }

        /// <summary>
        /// Returns a request builder with the provided arbitrary URL. Using this method means any other path or query parameters are ignored.
        /// </summary>
        /// <returns>A <see cref="global::OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Mansions.Item.MansionsItemRequestBuilder"/></returns>
        /// <param name="rawUrl">The raw URL to use for the request builder.</param>
        public global::OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Mansions.Item.MansionsItemRequestBuilder WithUrl(string rawUrl)
        {
            return new global::OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Mansions.Item.MansionsItemRequestBuilder(rawUrl, RequestAdapter);
        }

        /// <summary>
        /// Retrieves an individual mansion by its identifier.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCode("Kiota", "1.0.0")]
        public partial class MansionsItemRequestBuilderGetQueryParameters 
        {
            /// <summary>For syntax, see the documentation for the [`include`](https://www.jsonapi.net/usage/reading/including-relationships.html)/[`filter`](https://www.jsonapi.net/usage/reading/filtering.html)/[`sort`](https://www.jsonapi.net/usage/reading/sorting.html)/[`page`](https://www.jsonapi.net/usage/reading/pagination.html)/[`fields`](https://www.jsonapi.net/usage/reading/sparse-fieldset-selection.html) query string parameters.</summary>
            [QueryParameter("query")]
            public string? Query { get; set; }
        }

        /// <summary>
        /// Compare the returned ETag HTTP header with an earlier one to determine if the response has changed since it was fetched.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCode("Kiota", "1.0.0")]
        public partial class MansionsItemRequestBuilderHeadQueryParameters 
        {
            /// <summary>For syntax, see the documentation for the [`include`](https://www.jsonapi.net/usage/reading/including-relationships.html)/[`filter`](https://www.jsonapi.net/usage/reading/filtering.html)/[`sort`](https://www.jsonapi.net/usage/reading/sorting.html)/[`page`](https://www.jsonapi.net/usage/reading/pagination.html)/[`fields`](https://www.jsonapi.net/usage/reading/sparse-fieldset-selection.html) query string parameters.</summary>
            [QueryParameter("query")]
            public string? Query { get; set; }
        }

        /// <summary>
        /// Updates an existing mansion.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCode("Kiota", "1.0.0")]
        public partial class MansionsItemRequestBuilderPatchQueryParameters 
        {
            /// <summary>For syntax, see the documentation for the [`include`](https://www.jsonapi.net/usage/reading/including-relationships.html)/[`filter`](https://www.jsonapi.net/usage/reading/filtering.html)/[`sort`](https://www.jsonapi.net/usage/reading/sorting.html)/[`page`](https://www.jsonapi.net/usage/reading/pagination.html)/[`fields`](https://www.jsonapi.net/usage/reading/sparse-fieldset-selection.html) query string parameters.</summary>
            [QueryParameter("query")]
            public string? Query { get; set; }
        }
    }
}
#pragma warning restore CS0618
