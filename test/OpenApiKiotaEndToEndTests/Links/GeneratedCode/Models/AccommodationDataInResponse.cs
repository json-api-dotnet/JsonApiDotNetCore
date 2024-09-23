// <auto-generated/>
using Microsoft.Kiota.Abstractions.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
namespace OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models
{
    #pragma warning disable CS1591
    public class AccommodationDataInResponse : OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.DataInResponse, IParsable
    #pragma warning restore CS1591
    {
        /// <summary>The attributes property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.AccommodationAttributesInResponse? Attributes
        {
            get { return BackingStore?.Get<OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.AccommodationAttributesInResponse?>("attributes"); }
            set { BackingStore?.Set("attributes", value); }
        }
#nullable restore
#else
        public OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.AccommodationAttributesInResponse Attributes
        {
            get { return BackingStore?.Get<OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.AccommodationAttributesInResponse>("attributes"); }
            set { BackingStore?.Set("attributes", value); }
        }
#endif
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
        /// <summary>The links property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.ResourceLinks? Links
        {
            get { return BackingStore?.Get<OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.ResourceLinks?>("links"); }
            set { BackingStore?.Set("links", value); }
        }
#nullable restore
#else
        public OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.ResourceLinks Links
        {
            get { return BackingStore?.Get<OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.ResourceLinks>("links"); }
            set { BackingStore?.Set("links", value); }
        }
#endif
        /// <summary>The meta property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.Meta? Meta
        {
            get { return BackingStore?.Get<OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.Meta?>("meta"); }
            set { BackingStore?.Set("meta", value); }
        }
#nullable restore
#else
        public OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.Meta Meta
        {
            get { return BackingStore?.Get<OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.Meta>("meta"); }
            set { BackingStore?.Set("meta", value); }
        }
#endif
        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// </summary>
        /// <returns>A <see cref="OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.AccommodationDataInResponse"/></returns>
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        public static new OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.AccommodationDataInResponse CreateFromDiscriminatorValue(IParseNode parseNode)
        {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            return new OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.AccommodationDataInResponse();
        }
        /// <summary>
        /// The deserialization information for the current model
        /// </summary>
        /// <returns>A IDictionary&lt;string, Action&lt;IParseNode&gt;&gt;</returns>
        public override IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
        {
            return new Dictionary<string, Action<IParseNode>>(base.GetFieldDeserializers())
            {
                { "attributes", n => { Attributes = n.GetObjectValue<OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.AccommodationAttributesInResponse>(OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.AccommodationAttributesInResponse.CreateFromDiscriminatorValue); } },
                { "id", n => { Id = n.GetStringValue(); } },
                { "links", n => { Links = n.GetObjectValue<OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.ResourceLinks>(OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.ResourceLinks.CreateFromDiscriminatorValue); } },
                { "meta", n => { Meta = n.GetObjectValue<OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.Meta>(OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.Meta.CreateFromDiscriminatorValue); } },
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
            writer.WriteObjectValue<OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.AccommodationAttributesInResponse>("attributes", Attributes);
            writer.WriteStringValue("id", Id);
            writer.WriteObjectValue<OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.ResourceLinks>("links", Links);
            writer.WriteObjectValue<OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.Meta>("meta", Meta);
        }
    }
}