// <auto-generated/>
using Microsoft.Kiota.Abstractions;
using OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.RelationshipChannels.Item;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;
namespace OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.RelationshipChannels {
    /// <summary>
    /// Builds and executes requests for operations under \relationshipChannels
    /// </summary>
    public class RelationshipChannelsRequestBuilder : BaseRequestBuilder {
        /// <summary>Gets an item from the OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.relationshipChannels.item collection</summary>
        /// <param name="position">The identifier of the relationshipChannel whose related dataStream identities to retrieve.</param>
        public RelationshipChannelsItemRequestBuilder this[string position] { get {
            var urlTplParams = new Dictionary<string, object>(PathParameters);
            urlTplParams.Add("id", position);
            return new RelationshipChannelsItemRequestBuilder(urlTplParams, RequestAdapter);
        } }
        /// <summary>
        /// Instantiates a new RelationshipChannelsRequestBuilder and sets the default values.
        /// </summary>
        /// <param name="pathParameters">Path parameters for the request</param>
        /// <param name="requestAdapter">The request adapter to use to execute the requests.</param>
        public RelationshipChannelsRequestBuilder(Dictionary<string, object> pathParameters, IRequestAdapter requestAdapter) : base(requestAdapter, "{+baseurl}/relationshipChannels", pathParameters) {
        }
        /// <summary>
        /// Instantiates a new RelationshipChannelsRequestBuilder and sets the default values.
        /// </summary>
        /// <param name="rawUrl">The raw URL to use for the request builder.</param>
        /// <param name="requestAdapter">The request adapter to use to execute the requests.</param>
        public RelationshipChannelsRequestBuilder(string rawUrl, IRequestAdapter requestAdapter) : base(requestAdapter, "{+baseurl}/relationshipChannels", rawUrl) {
        }
    }
}