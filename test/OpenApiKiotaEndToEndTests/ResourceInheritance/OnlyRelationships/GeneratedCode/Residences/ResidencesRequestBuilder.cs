// <auto-generated/>
#nullable enable
#pragma warning disable CS8625
#pragma warning disable CS0618
using Microsoft.Kiota.Abstractions.Extensions;
using Microsoft.Kiota.Abstractions;
using OpenApiKiotaEndToEndTests.ResourceInheritance.OnlyRelationships.GeneratedCode.Residences.Item;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System;
namespace OpenApiKiotaEndToEndTests.ResourceInheritance.OnlyRelationships.GeneratedCode.Residences
{
    /// <summary>
    /// Builds and executes requests for operations under \residences
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCode("Kiota", "1.0.0")]
    public partial class ResidencesRequestBuilder : BaseRequestBuilder
    {
        /// <summary>Gets an item from the OpenApiKiotaEndToEndTests.ResourceInheritance.OnlyRelationships.GeneratedCode.residences.item collection</summary>
        /// <param name="position">The identifier of the residence whose related room identities to retrieve.</param>
        /// <returns>A <see cref="global::OpenApiKiotaEndToEndTests.ResourceInheritance.OnlyRelationships.GeneratedCode.Residences.Item.ResidencesItemRequestBuilder"/></returns>
        public global::OpenApiKiotaEndToEndTests.ResourceInheritance.OnlyRelationships.GeneratedCode.Residences.Item.ResidencesItemRequestBuilder this[string position]
        {
            get
            {
                var urlTplParams = new Dictionary<string, object>(PathParameters);
                urlTplParams.Add("id", position);
                return new global::OpenApiKiotaEndToEndTests.ResourceInheritance.OnlyRelationships.GeneratedCode.Residences.Item.ResidencesItemRequestBuilder(urlTplParams, RequestAdapter);
            }
        }

        /// <summary>
        /// Instantiates a new <see cref="global::OpenApiKiotaEndToEndTests.ResourceInheritance.OnlyRelationships.GeneratedCode.Residences.ResidencesRequestBuilder"/> and sets the default values.
        /// </summary>
        /// <param name="pathParameters">Path parameters for the request</param>
        /// <param name="requestAdapter">The request adapter to use to execute the requests.</param>
        public ResidencesRequestBuilder(Dictionary<string, object> pathParameters, IRequestAdapter requestAdapter) : base(requestAdapter, "{+baseurl}/residences", pathParameters)
        {
        }

        /// <summary>
        /// Instantiates a new <see cref="global::OpenApiKiotaEndToEndTests.ResourceInheritance.OnlyRelationships.GeneratedCode.Residences.ResidencesRequestBuilder"/> and sets the default values.
        /// </summary>
        /// <param name="rawUrl">The raw URL to use for the request builder.</param>
        /// <param name="requestAdapter">The request adapter to use to execute the requests.</param>
        public ResidencesRequestBuilder(string rawUrl, IRequestAdapter requestAdapter) : base(requestAdapter, "{+baseurl}/residences", rawUrl)
        {
        }
    }
}
#pragma warning restore CS0618
