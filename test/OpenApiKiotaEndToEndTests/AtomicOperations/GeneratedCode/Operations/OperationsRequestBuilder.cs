// <auto-generated/>
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions;
using OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;
namespace OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Operations
{
    /// <summary>
    /// Builds and executes requests for operations under \operations
    /// </summary>
    public class OperationsRequestBuilder : BaseRequestBuilder
    {
        /// <summary>
        /// Instantiates a new <see cref="OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Operations.OperationsRequestBuilder"/> and sets the default values.
        /// </summary>
        /// <param name="pathParameters">Path parameters for the request</param>
        /// <param name="requestAdapter">The request adapter to use to execute the requests.</param>
        public OperationsRequestBuilder(Dictionary<string, object> pathParameters, IRequestAdapter requestAdapter) : base(requestAdapter, "{+baseurl}/operations", pathParameters)
        {
        }
        /// <summary>
        /// Instantiates a new <see cref="OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Operations.OperationsRequestBuilder"/> and sets the default values.
        /// </summary>
        /// <param name="rawUrl">The raw URL to use for the request builder.</param>
        /// <param name="requestAdapter">The request adapter to use to execute the requests.</param>
        public OperationsRequestBuilder(string rawUrl, IRequestAdapter requestAdapter) : base(requestAdapter, "{+baseurl}/operations", rawUrl)
        {
        }
        /// <summary>
        /// Performs multiple mutations in a linear and atomic manner.
        /// </summary>
        /// <returns>A <see cref="OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.OperationsResponseDocument"/></returns>
        /// <param name="body">The request body</param>
        /// <param name="cancellationToken">Cancellation token to use when cancelling requests</param>
        /// <param name="requestConfiguration">Configuration for the request such as headers, query parameters, and middleware options.</param>
        /// <exception cref="OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.ErrorResponseDocument">When receiving a 400 status code</exception>
        /// <exception cref="OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.ErrorResponseDocument">When receiving a 403 status code</exception>
        /// <exception cref="OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.ErrorResponseDocument">When receiving a 404 status code</exception>
        /// <exception cref="OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.ErrorResponseDocument">When receiving a 409 status code</exception>
        /// <exception cref="OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.ErrorResponseDocument">When receiving a 422 status code</exception>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public async Task<OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.OperationsResponseDocument?> PostAsync(OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.OperationsRequestDocument body, Action<RequestConfiguration<DefaultQueryParameters>>? requestConfiguration = default, CancellationToken cancellationToken = default)
        {
#nullable restore
#else
        public async Task<OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.OperationsResponseDocument> PostAsync(OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.OperationsRequestDocument body, Action<RequestConfiguration<DefaultQueryParameters>> requestConfiguration = default, CancellationToken cancellationToken = default)
        {
#endif
            _ = body ?? throw new ArgumentNullException(nameof(body));
            var requestInfo = ToPostRequestInformation(body, requestConfiguration);
            var errorMapping = new Dictionary<string, ParsableFactory<IParsable>>
            {
                { "400", OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.ErrorResponseDocument.CreateFromDiscriminatorValue },
                { "403", OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.ErrorResponseDocument.CreateFromDiscriminatorValue },
                { "404", OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.ErrorResponseDocument.CreateFromDiscriminatorValue },
                { "409", OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.ErrorResponseDocument.CreateFromDiscriminatorValue },
                { "422", OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.ErrorResponseDocument.CreateFromDiscriminatorValue },
            };
            return await RequestAdapter.SendAsync<OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.OperationsResponseDocument>(requestInfo, OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.OperationsResponseDocument.CreateFromDiscriminatorValue, errorMapping, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Performs multiple mutations in a linear and atomic manner.
        /// </summary>
        /// <returns>A <see cref="RequestInformation"/></returns>
        /// <param name="body">The request body</param>
        /// <param name="requestConfiguration">Configuration for the request such as headers, query parameters, and middleware options.</param>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public RequestInformation ToPostRequestInformation(OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.OperationsRequestDocument body, Action<RequestConfiguration<DefaultQueryParameters>>? requestConfiguration = default)
        {
#nullable restore
#else
        public RequestInformation ToPostRequestInformation(OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.OperationsRequestDocument body, Action<RequestConfiguration<DefaultQueryParameters>> requestConfiguration = default)
        {
#endif
            _ = body ?? throw new ArgumentNullException(nameof(body));
            var requestInfo = new RequestInformation(Method.POST, UrlTemplate, PathParameters);
            requestInfo.Configure(requestConfiguration);
            requestInfo.Headers.TryAdd("Accept", "application/vnd.api+json;ext=atomic-operations");
            requestInfo.SetContentFromParsable(RequestAdapter, "application/vnd.api+json;ext=atomic-operations", body);
            return requestInfo;
        }
        /// <summary>
        /// Returns a request builder with the provided arbitrary URL. Using this method means any other path or query parameters are ignored.
        /// </summary>
        /// <returns>A <see cref="OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Operations.OperationsRequestBuilder"/></returns>
        /// <param name="rawUrl">The raw URL to use for the request builder.</param>
        public OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Operations.OperationsRequestBuilder WithUrl(string rawUrl)
        {
            return new OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Operations.OperationsRequestBuilder(rawUrl, RequestAdapter);
        }
    }
}