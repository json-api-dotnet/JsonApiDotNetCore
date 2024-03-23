// <auto-generated/>
using Microsoft.Kiota.Abstractions.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
namespace OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.Models {
    public class ReadOnlyResourceChannelDataInResponse : DataInResponse, IParsable {
        /// <summary>The attributes property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public ReadOnlyResourceChannelAttributesInResponse? Attributes {
            get { return BackingStore?.Get<ReadOnlyResourceChannelAttributesInResponse?>("attributes"); }
            set { BackingStore?.Set("attributes", value); }
        }
#nullable restore
#else
        public ReadOnlyResourceChannelAttributesInResponse Attributes {
            get { return BackingStore?.Get<ReadOnlyResourceChannelAttributesInResponse>("attributes"); }
            set { BackingStore?.Set("attributes", value); }
        }
#endif
        /// <summary>The links property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public LinksInResourceData? Links {
            get { return BackingStore?.Get<LinksInResourceData?>("links"); }
            set { BackingStore?.Set("links", value); }
        }
#nullable restore
#else
        public LinksInResourceData Links {
            get { return BackingStore?.Get<LinksInResourceData>("links"); }
            set { BackingStore?.Set("links", value); }
        }
#endif
        /// <summary>The meta property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public ReadOnlyResourceChannelDataInResponse_meta? Meta {
            get { return BackingStore?.Get<ReadOnlyResourceChannelDataInResponse_meta?>("meta"); }
            set { BackingStore?.Set("meta", value); }
        }
#nullable restore
#else
        public ReadOnlyResourceChannelDataInResponse_meta Meta {
            get { return BackingStore?.Get<ReadOnlyResourceChannelDataInResponse_meta>("meta"); }
            set { BackingStore?.Set("meta", value); }
        }
#endif
        /// <summary>The relationships property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public ReadOnlyResourceChannelRelationshipsInResponse? Relationships {
            get { return BackingStore?.Get<ReadOnlyResourceChannelRelationshipsInResponse?>("relationships"); }
            set { BackingStore?.Set("relationships", value); }
        }
#nullable restore
#else
        public ReadOnlyResourceChannelRelationshipsInResponse Relationships {
            get { return BackingStore?.Get<ReadOnlyResourceChannelRelationshipsInResponse>("relationships"); }
            set { BackingStore?.Set("relationships", value); }
        }
#endif
        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// </summary>
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        public static new ReadOnlyResourceChannelDataInResponse CreateFromDiscriminatorValue(IParseNode parseNode) {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            return new ReadOnlyResourceChannelDataInResponse();
        }
        /// <summary>
        /// The deserialization information for the current model
        /// </summary>
        public override IDictionary<string, Action<IParseNode>> GetFieldDeserializers() {
            return new Dictionary<string, Action<IParseNode>>(base.GetFieldDeserializers()) {
                {"attributes", n => { Attributes = n.GetObjectValue<ReadOnlyResourceChannelAttributesInResponse>(ReadOnlyResourceChannelAttributesInResponse.CreateFromDiscriminatorValue); } },
                {"links", n => { Links = n.GetObjectValue<LinksInResourceData>(LinksInResourceData.CreateFromDiscriminatorValue); } },
                {"meta", n => { Meta = n.GetObjectValue<ReadOnlyResourceChannelDataInResponse_meta>(ReadOnlyResourceChannelDataInResponse_meta.CreateFromDiscriminatorValue); } },
                {"relationships", n => { Relationships = n.GetObjectValue<ReadOnlyResourceChannelRelationshipsInResponse>(ReadOnlyResourceChannelRelationshipsInResponse.CreateFromDiscriminatorValue); } },
            };
        }
        /// <summary>
        /// Serializes information the current object
        /// </summary>
        /// <param name="writer">Serialization writer to use to serialize this model</param>
        public override void Serialize(ISerializationWriter writer) {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            base.Serialize(writer);
            writer.WriteObjectValue<ReadOnlyResourceChannelAttributesInResponse>("attributes", Attributes);
            writer.WriteObjectValue<LinksInResourceData>("links", Links);
            writer.WriteObjectValue<ReadOnlyResourceChannelDataInResponse_meta>("meta", Meta);
            writer.WriteObjectValue<ReadOnlyResourceChannelRelationshipsInResponse>("relationships", Relationships);
        }
    }
}