// <auto-generated/>
#pragma warning disable CS0618
using Microsoft.Kiota.Abstractions.Extensions;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions.Store;
using System.Collections.Generic;
using System.IO;
using System;
namespace OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models
{
    [global::System.CodeDom.Compiler.GeneratedCode("Kiota", "1.0.0")]
    #pragma warning disable CS1591
    public partial class DataInUpdateTransportRequest : IBackedModel, IParsable
    #pragma warning restore CS1591
    {
        /// <summary>The attributes property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.AttributesInUpdateTransportRequest? Attributes
        {
            get { return BackingStore?.Get<global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.AttributesInUpdateTransportRequest?>("attributes"); }
            set { BackingStore?.Set("attributes", value); }
        }
#nullable restore
#else
        public global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.AttributesInUpdateTransportRequest Attributes
        {
            get { return BackingStore?.Get<global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.AttributesInUpdateTransportRequest>("attributes"); }
            set { BackingStore?.Set("attributes", value); }
        }
#endif
        /// <summary>Stores model information.</summary>
        public IBackingStore BackingStore { get; private set; }
        /// <summary>The id property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public string? Id
        {
            get { return BackingStore?.Get<string?>("id"); }
            set { BackingStore?.Set("id", value); }
        }
#nullable restore
#else
        public string Id
        {
            get { return BackingStore?.Get<string>("id"); }
            set { BackingStore?.Set("id", value); }
        }
#endif
        /// <summary>The meta property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.Meta? Meta
        {
            get { return BackingStore?.Get<global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.Meta?>("meta"); }
            set { BackingStore?.Set("meta", value); }
        }
#nullable restore
#else
        public global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.Meta Meta
        {
            get { return BackingStore?.Get<global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.Meta>("meta"); }
            set { BackingStore?.Set("meta", value); }
        }
#endif
        /// <summary>The type property</summary>
        public global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.TransportResourceType? Type
        {
            get { return BackingStore?.Get<global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.TransportResourceType?>("type"); }
            set { BackingStore?.Set("type", value); }
        }
        /// <summary>
        /// Instantiates a new <see cref="global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.DataInUpdateTransportRequest"/> and sets the default values.
        /// </summary>
        public DataInUpdateTransportRequest()
        {
            BackingStore = BackingStoreFactorySingleton.Instance.CreateBackingStore();
        }
        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// </summary>
        /// <returns>A <see cref="global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.DataInUpdateTransportRequest"/></returns>
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        public static global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.DataInUpdateTransportRequest CreateFromDiscriminatorValue(IParseNode parseNode)
        {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            return new global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.DataInUpdateTransportRequest();
        }
        /// <summary>
        /// The deserialization information for the current model
        /// </summary>
        /// <returns>A IDictionary&lt;string, Action&lt;IParseNode&gt;&gt;</returns>
        public virtual IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
        {
            return new Dictionary<string, Action<IParseNode>>
            {
                { "attributes", n => { Attributes = n.GetObjectValue<global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.AttributesInUpdateTransportRequest>(global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.AttributesInUpdateTransportRequest.CreateFromDiscriminatorValue); } },
                { "id", n => { Id = n.GetStringValue(); } },
                { "meta", n => { Meta = n.GetObjectValue<global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.Meta>(global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.Meta.CreateFromDiscriminatorValue); } },
                { "type", n => { Type = n.GetEnumValue<global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.TransportResourceType>(); } },
            };
        }
        /// <summary>
        /// Serializes information the current object
        /// </summary>
        /// <param name="writer">Serialization writer to use to serialize this model</param>
        public virtual void Serialize(ISerializationWriter writer)
        {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            writer.WriteObjectValue<global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.AttributesInUpdateTransportRequest>("attributes", Attributes);
            writer.WriteStringValue("id", Id);
            writer.WriteObjectValue<global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.Meta>("meta", Meta);
            writer.WriteEnumValue<global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.TransportResourceType>("type", Type);
        }
    }
}
#pragma warning restore CS0618
