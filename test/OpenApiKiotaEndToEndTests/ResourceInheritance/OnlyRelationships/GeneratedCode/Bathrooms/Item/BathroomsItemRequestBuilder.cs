// <auto-generated/>
#nullable enable
#pragma warning disable CS8625
#pragma warning disable CS0618
using Microsoft.Kiota.Abstractions.Extensions;
using Microsoft.Kiota.Abstractions;
using OpenApiKiotaEndToEndTests.ResourceInheritance.OnlyRelationships.GeneratedCode.Bathrooms.Item.Relationships;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System;
namespace OpenApiKiotaEndToEndTests.ResourceInheritance.OnlyRelationships.GeneratedCode.Bathrooms.Item
{
    /// <summary>
    /// Builds and executes requests for operations under \bathrooms\{id}
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCode("Kiota", "1.0.0")]
    public partial class BathroomsItemRequestBuilder : BaseRequestBuilder
    {
        /// <summary>The relationships property</summary>
        public global::OpenApiKiotaEndToEndTests.ResourceInheritance.OnlyRelationships.GeneratedCode.Bathrooms.Item.Relationships.RelationshipsRequestBuilder Relationships
        {
            get => new global::OpenApiKiotaEndToEndTests.ResourceInheritance.OnlyRelationships.GeneratedCode.Bathrooms.Item.Relationships.RelationshipsRequestBuilder(PathParameters, RequestAdapter);
        }

        /// <summary>
        /// Instantiates a new <see cref="global::OpenApiKiotaEndToEndTests.ResourceInheritance.OnlyRelationships.GeneratedCode.Bathrooms.Item.BathroomsItemRequestBuilder"/> and sets the default values.
        /// </summary>
        /// <param name="pathParameters">Path parameters for the request</param>
        /// <param name="requestAdapter">The request adapter to use to execute the requests.</param>
        public BathroomsItemRequestBuilder(Dictionary<string, object> pathParameters, IRequestAdapter requestAdapter) : base(requestAdapter, "{+baseurl}/bathrooms/{id}", pathParameters)
        {
        }

        /// <summary>
        /// Instantiates a new <see cref="global::OpenApiKiotaEndToEndTests.ResourceInheritance.OnlyRelationships.GeneratedCode.Bathrooms.Item.BathroomsItemRequestBuilder"/> and sets the default values.
        /// </summary>
        /// <param name="rawUrl">The raw URL to use for the request builder.</param>
        /// <param name="requestAdapter">The request adapter to use to execute the requests.</param>
        public BathroomsItemRequestBuilder(string rawUrl, IRequestAdapter requestAdapter) : base(requestAdapter, "{+baseurl}/bathrooms/{id}", rawUrl)
        {
        }
    }
}
#pragma warning restore CS0618
