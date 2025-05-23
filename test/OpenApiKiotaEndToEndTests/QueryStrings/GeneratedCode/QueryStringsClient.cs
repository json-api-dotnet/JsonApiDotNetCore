// <auto-generated/>
#nullable enable
#pragma warning disable CS8625
#pragma warning disable CS0618
using Microsoft.Kiota.Abstractions.Extensions;
using Microsoft.Kiota.Abstractions.Store;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Serialization.Form;
using Microsoft.Kiota.Serialization.Json;
using Microsoft.Kiota.Serialization.Multipart;
using Microsoft.Kiota.Serialization.Text;
using OpenApiKiotaEndToEndTests.QueryStrings.GeneratedCode.NameValuePairs;
using OpenApiKiotaEndToEndTests.QueryStrings.GeneratedCode.Nodes;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System;
namespace OpenApiKiotaEndToEndTests.QueryStrings.GeneratedCode
{
    /// <summary>
    /// The main entry point of the SDK, exposes the configuration and the fluent API.
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCode("Kiota", "1.0.0")]
    public partial class QueryStringsClient : BaseRequestBuilder
    {
        /// <summary>The nameValuePairs property</summary>
        public global::OpenApiKiotaEndToEndTests.QueryStrings.GeneratedCode.NameValuePairs.NameValuePairsRequestBuilder NameValuePairs
        {
            get => new global::OpenApiKiotaEndToEndTests.QueryStrings.GeneratedCode.NameValuePairs.NameValuePairsRequestBuilder(PathParameters, RequestAdapter);
        }

        /// <summary>The nodes property</summary>
        public global::OpenApiKiotaEndToEndTests.QueryStrings.GeneratedCode.Nodes.NodesRequestBuilder Nodes
        {
            get => new global::OpenApiKiotaEndToEndTests.QueryStrings.GeneratedCode.Nodes.NodesRequestBuilder(PathParameters, RequestAdapter);
        }

        /// <summary>
        /// Instantiates a new <see cref="global::OpenApiKiotaEndToEndTests.QueryStrings.GeneratedCode.QueryStringsClient"/> and sets the default values.
        /// </summary>
        /// <param name="backingStore">The backing store to use for the models.</param>
        /// <param name="requestAdapter">The request adapter to use to execute the requests.</param>
        public QueryStringsClient(IRequestAdapter requestAdapter, IBackingStoreFactory backingStore = default) : base(requestAdapter, "{+baseurl}", new Dictionary<string, object>())
        {
            ApiClientBuilder.RegisterDefaultSerializer<JsonSerializationWriterFactory>();
            ApiClientBuilder.RegisterDefaultSerializer<TextSerializationWriterFactory>();
            ApiClientBuilder.RegisterDefaultSerializer<FormSerializationWriterFactory>();
            ApiClientBuilder.RegisterDefaultSerializer<MultipartSerializationWriterFactory>();
            ApiClientBuilder.RegisterDefaultDeserializer<JsonParseNodeFactory>();
            ApiClientBuilder.RegisterDefaultDeserializer<TextParseNodeFactory>();
            ApiClientBuilder.RegisterDefaultDeserializer<FormParseNodeFactory>();
            if (string.IsNullOrEmpty(RequestAdapter.BaseUrl))
            {
                RequestAdapter.BaseUrl = "http://localhost";
            }
            PathParameters.TryAdd("baseurl", RequestAdapter.BaseUrl);
            RequestAdapter.EnableBackingStore(backingStore);
        }
    }
}
#pragma warning restore CS0618
