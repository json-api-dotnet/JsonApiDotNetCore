// <auto-generated/>
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions;
using OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.Models;
using OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.ReadOnlyChannels.Item.AudioStreams;
using OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.ReadOnlyChannels.Item.Relationships;
using OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.ReadOnlyChannels.Item.UltraHighDefinitionVideoStream;
using OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.ReadOnlyChannels.Item.VideoStream;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;
namespace OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.ReadOnlyChannels.Item
{
    /// <summary>
    /// Builds and executes requests for operations under \readOnlyChannels\{id}
    /// </summary>
    public class ReadOnlyChannelsItemRequestBuilder : BaseRequestBuilder
    {
        /// <summary>The audioStreams property</summary>
        public OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.ReadOnlyChannels.Item.AudioStreams.AudioStreamsRequestBuilder AudioStreams
        {
            get => new OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.ReadOnlyChannels.Item.AudioStreams.AudioStreamsRequestBuilder(PathParameters, RequestAdapter);
        }
        /// <summary>The relationships property</summary>
        public OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.ReadOnlyChannels.Item.Relationships.RelationshipsRequestBuilder Relationships
        {
            get => new OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.ReadOnlyChannels.Item.Relationships.RelationshipsRequestBuilder(PathParameters, RequestAdapter);
        }
        /// <summary>The ultraHighDefinitionVideoStream property</summary>
        public OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.ReadOnlyChannels.Item.UltraHighDefinitionVideoStream.UltraHighDefinitionVideoStreamRequestBuilder UltraHighDefinitionVideoStream
        {
            get => new OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.ReadOnlyChannels.Item.UltraHighDefinitionVideoStream.UltraHighDefinitionVideoStreamRequestBuilder(PathParameters, RequestAdapter);
        }
        /// <summary>The videoStream property</summary>
        public OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.ReadOnlyChannels.Item.VideoStream.VideoStreamRequestBuilder VideoStream
        {
            get => new OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.ReadOnlyChannels.Item.VideoStream.VideoStreamRequestBuilder(PathParameters, RequestAdapter);
        }
        /// <summary>
        /// Instantiates a new <see cref="OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.ReadOnlyChannels.Item.ReadOnlyChannelsItemRequestBuilder"/> and sets the default values.
        /// </summary>
        /// <param name="pathParameters">Path parameters for the request</param>
        /// <param name="requestAdapter">The request adapter to use to execute the requests.</param>
        public ReadOnlyChannelsItemRequestBuilder(Dictionary<string, object> pathParameters, IRequestAdapter requestAdapter) : base(requestAdapter, "{+baseurl}/readOnlyChannels/{id}{?query*}", pathParameters)
        {
        }
        /// <summary>
        /// Instantiates a new <see cref="OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.ReadOnlyChannels.Item.ReadOnlyChannelsItemRequestBuilder"/> and sets the default values.
        /// </summary>
        /// <param name="rawUrl">The raw URL to use for the request builder.</param>
        /// <param name="requestAdapter">The request adapter to use to execute the requests.</param>
        public ReadOnlyChannelsItemRequestBuilder(string rawUrl, IRequestAdapter requestAdapter) : base(requestAdapter, "{+baseurl}/readOnlyChannels/{id}{?query*}", rawUrl)
        {
        }
        /// <summary>
        /// Retrieves an individual readOnlyChannel by its identifier.
        /// </summary>
        /// <returns>A <see cref="OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.Models.ReadOnlyChannelPrimaryResponseDocument"/></returns>
        /// <param name="cancellationToken">Cancellation token to use when cancelling requests</param>
        /// <param name="requestConfiguration">Configuration for the request such as headers, query parameters, and middleware options.</param>
        /// <exception cref="OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.Models.ErrorResponseDocument">When receiving a 400 status code</exception>
        /// <exception cref="OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.Models.ErrorResponseDocument">When receiving a 404 status code</exception>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public async Task<OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.Models.ReadOnlyChannelPrimaryResponseDocument?> GetAsync(Action<RequestConfiguration<OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.ReadOnlyChannels.Item.ReadOnlyChannelsItemRequestBuilder.ReadOnlyChannelsItemRequestBuilderGetQueryParameters>>? requestConfiguration = default, CancellationToken cancellationToken = default)
        {
#nullable restore
#else
        public async Task<OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.Models.ReadOnlyChannelPrimaryResponseDocument> GetAsync(Action<RequestConfiguration<OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.ReadOnlyChannels.Item.ReadOnlyChannelsItemRequestBuilder.ReadOnlyChannelsItemRequestBuilderGetQueryParameters>> requestConfiguration = default, CancellationToken cancellationToken = default)
        {
#endif
            var requestInfo = ToGetRequestInformation(requestConfiguration);
            var errorMapping = new Dictionary<string, ParsableFactory<IParsable>>
            {
                { "400", OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.Models.ErrorResponseDocument.CreateFromDiscriminatorValue },
                { "404", OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.Models.ErrorResponseDocument.CreateFromDiscriminatorValue },
            };
            return await RequestAdapter.SendAsync<OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.Models.ReadOnlyChannelPrimaryResponseDocument>(requestInfo, OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.Models.ReadOnlyChannelPrimaryResponseDocument.CreateFromDiscriminatorValue, errorMapping, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Compare the returned ETag HTTP header with an earlier one to determine if the response has changed since it was fetched.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to use when cancelling requests</param>
        /// <param name="requestConfiguration">Configuration for the request such as headers, query parameters, and middleware options.</param>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public async Task HeadAsync(Action<RequestConfiguration<OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.ReadOnlyChannels.Item.ReadOnlyChannelsItemRequestBuilder.ReadOnlyChannelsItemRequestBuilderHeadQueryParameters>>? requestConfiguration = default, CancellationToken cancellationToken = default)
        {
#nullable restore
#else
        public async Task HeadAsync(Action<RequestConfiguration<OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.ReadOnlyChannels.Item.ReadOnlyChannelsItemRequestBuilder.ReadOnlyChannelsItemRequestBuilderHeadQueryParameters>> requestConfiguration = default, CancellationToken cancellationToken = default)
        {
#endif
            var requestInfo = ToHeadRequestInformation(requestConfiguration);
            await RequestAdapter.SendNoContentAsync(requestInfo, default, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Retrieves an individual readOnlyChannel by its identifier.
        /// </summary>
        /// <returns>A <see cref="RequestInformation"/></returns>
        /// <param name="requestConfiguration">Configuration for the request such as headers, query parameters, and middleware options.</param>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public RequestInformation ToGetRequestInformation(Action<RequestConfiguration<OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.ReadOnlyChannels.Item.ReadOnlyChannelsItemRequestBuilder.ReadOnlyChannelsItemRequestBuilderGetQueryParameters>>? requestConfiguration = default)
        {
#nullable restore
#else
        public RequestInformation ToGetRequestInformation(Action<RequestConfiguration<OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.ReadOnlyChannels.Item.ReadOnlyChannelsItemRequestBuilder.ReadOnlyChannelsItemRequestBuilderGetQueryParameters>> requestConfiguration = default)
        {
#endif
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
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public RequestInformation ToHeadRequestInformation(Action<RequestConfiguration<OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.ReadOnlyChannels.Item.ReadOnlyChannelsItemRequestBuilder.ReadOnlyChannelsItemRequestBuilderHeadQueryParameters>>? requestConfiguration = default)
        {
#nullable restore
#else
        public RequestInformation ToHeadRequestInformation(Action<RequestConfiguration<OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.ReadOnlyChannels.Item.ReadOnlyChannelsItemRequestBuilder.ReadOnlyChannelsItemRequestBuilderHeadQueryParameters>> requestConfiguration = default)
        {
#endif
            var requestInfo = new RequestInformation(Method.HEAD, UrlTemplate, PathParameters);
            requestInfo.Configure(requestConfiguration);
            return requestInfo;
        }
        /// <summary>
        /// Returns a request builder with the provided arbitrary URL. Using this method means any other path or query parameters are ignored.
        /// </summary>
        /// <returns>A <see cref="OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.ReadOnlyChannels.Item.ReadOnlyChannelsItemRequestBuilder"/></returns>
        /// <param name="rawUrl">The raw URL to use for the request builder.</param>
        public OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.ReadOnlyChannels.Item.ReadOnlyChannelsItemRequestBuilder WithUrl(string rawUrl)
        {
            return new OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.ReadOnlyChannels.Item.ReadOnlyChannelsItemRequestBuilder(rawUrl, RequestAdapter);
        }
        /// <summary>
        /// Retrieves an individual readOnlyChannel by its identifier.
        /// </summary>
        public class ReadOnlyChannelsItemRequestBuilderGetQueryParameters 
        {
            /// <summary>For syntax, see the documentation for the [`include`](https://www.jsonapi.net/usage/reading/including-relationships.html)/[`filter`](https://www.jsonapi.net/usage/reading/filtering.html)/[`sort`](https://www.jsonapi.net/usage/reading/sorting.html)/[`page`](https://www.jsonapi.net/usage/reading/pagination.html)/[`fields`](https://www.jsonapi.net/usage/reading/sparse-fieldset-selection.html) query string parameters.</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
            [QueryParameter("query")]
            public string? Query { get; set; }
#nullable restore
#else
            [QueryParameter("query")]
            public string Query { get; set; }
#endif
        }
        /// <summary>
        /// Compare the returned ETag HTTP header with an earlier one to determine if the response has changed since it was fetched.
        /// </summary>
        public class ReadOnlyChannelsItemRequestBuilderHeadQueryParameters 
        {
            /// <summary>For syntax, see the documentation for the [`include`](https://www.jsonapi.net/usage/reading/including-relationships.html)/[`filter`](https://www.jsonapi.net/usage/reading/filtering.html)/[`sort`](https://www.jsonapi.net/usage/reading/sorting.html)/[`page`](https://www.jsonapi.net/usage/reading/pagination.html)/[`fields`](https://www.jsonapi.net/usage/reading/sparse-fieldset-selection.html) query string parameters.</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
            [QueryParameter("query")]
            public string? Query { get; set; }
#nullable restore
#else
            [QueryParameter("query")]
            public string Query { get; set; }
#endif
        }
    }
}