// <auto-generated/>
#nullable enable
#pragma warning disable CS8625
#pragma warning disable CS0618
using Microsoft.Kiota.Abstractions.Extensions;
using Microsoft.Kiota.Abstractions.Serialization;
using System.Collections.Generic;
using System.IO;
using System;
namespace OpenApiKiotaEndToEndTests.QueryStrings.GeneratedCode.Models
{
    [global::System.CodeDom.Compiler.GeneratedCode("Kiota", "1.0.0")]
    #pragma warning disable CS1591
    public partial class RelationshipsInCreateNodeRequest : global::OpenApiKiotaEndToEndTests.QueryStrings.GeneratedCode.Models.RelationshipsInCreateRequest, IParsable
    #pragma warning restore CS1591
    {
        /// <summary>The children property</summary>
        public global::OpenApiKiotaEndToEndTests.QueryStrings.GeneratedCode.Models.ToManyNodeInRequest? Children
        {
            get { return BackingStore?.Get<global::OpenApiKiotaEndToEndTests.QueryStrings.GeneratedCode.Models.ToManyNodeInRequest?>("children"); }
            set { BackingStore?.Set("children", value); }
        }

        /// <summary>The parent property</summary>
        public global::OpenApiKiotaEndToEndTests.QueryStrings.GeneratedCode.Models.NullableToOneNodeInRequest? Parent
        {
            get { return BackingStore?.Get<global::OpenApiKiotaEndToEndTests.QueryStrings.GeneratedCode.Models.NullableToOneNodeInRequest?>("parent"); }
            set { BackingStore?.Set("parent", value); }
        }

        /// <summary>The values property</summary>
        public global::OpenApiKiotaEndToEndTests.QueryStrings.GeneratedCode.Models.ToManyNameValuePairInRequest? Values
        {
            get { return BackingStore?.Get<global::OpenApiKiotaEndToEndTests.QueryStrings.GeneratedCode.Models.ToManyNameValuePairInRequest?>("values"); }
            set { BackingStore?.Set("values", value); }
        }

        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// </summary>
        /// <returns>A <see cref="global::OpenApiKiotaEndToEndTests.QueryStrings.GeneratedCode.Models.RelationshipsInCreateNodeRequest"/></returns>
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        public static new global::OpenApiKiotaEndToEndTests.QueryStrings.GeneratedCode.Models.RelationshipsInCreateNodeRequest CreateFromDiscriminatorValue(IParseNode parseNode)
        {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            return new global::OpenApiKiotaEndToEndTests.QueryStrings.GeneratedCode.Models.RelationshipsInCreateNodeRequest();
        }

        /// <summary>
        /// The deserialization information for the current model
        /// </summary>
        /// <returns>A IDictionary&lt;string, Action&lt;IParseNode&gt;&gt;</returns>
        public override IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
        {
            return new Dictionary<string, Action<IParseNode>>(base.GetFieldDeserializers())
            {
                { "children", n => { Children = n.GetObjectValue<global::OpenApiKiotaEndToEndTests.QueryStrings.GeneratedCode.Models.ToManyNodeInRequest>(global::OpenApiKiotaEndToEndTests.QueryStrings.GeneratedCode.Models.ToManyNodeInRequest.CreateFromDiscriminatorValue); } },
                { "parent", n => { Parent = n.GetObjectValue<global::OpenApiKiotaEndToEndTests.QueryStrings.GeneratedCode.Models.NullableToOneNodeInRequest>(global::OpenApiKiotaEndToEndTests.QueryStrings.GeneratedCode.Models.NullableToOneNodeInRequest.CreateFromDiscriminatorValue); } },
                { "values", n => { Values = n.GetObjectValue<global::OpenApiKiotaEndToEndTests.QueryStrings.GeneratedCode.Models.ToManyNameValuePairInRequest>(global::OpenApiKiotaEndToEndTests.QueryStrings.GeneratedCode.Models.ToManyNameValuePairInRequest.CreateFromDiscriminatorValue); } },
            };
        }

        /// <summary>
        /// Serializes information the current object
        /// </summary>
        /// <param name="writer">Serialization writer to use to serialize this model</param>
        public override void Serialize(ISerializationWriter writer)
        {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            base.Serialize(writer);
            writer.WriteObjectValue<global::OpenApiKiotaEndToEndTests.QueryStrings.GeneratedCode.Models.ToManyNodeInRequest>("children", Children);
            writer.WriteObjectValue<global::OpenApiKiotaEndToEndTests.QueryStrings.GeneratedCode.Models.NullableToOneNodeInRequest>("parent", Parent);
            writer.WriteObjectValue<global::OpenApiKiotaEndToEndTests.QueryStrings.GeneratedCode.Models.ToManyNameValuePairInRequest>("values", Values);
        }
    }
}
#pragma warning restore CS0618
