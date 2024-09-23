// <auto-generated/>
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions.Store;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
namespace OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.Models
{
    #pragma warning disable CS1591
    public class DataInUpdateWriteOnlyChannelRequest : IBackedModel, IParsable
    #pragma warning restore CS1591
    {
        /// <summary>The attributes property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.Models.AttributesInUpdateWriteOnlyChannelRequest? Attributes
        {
            get { return BackingStore?.Get<OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.Models.AttributesInUpdateWriteOnlyChannelRequest?>("attributes"); }
            set { BackingStore?.Set("attributes", value); }
        }
#nullable restore
#else
        public OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.Models.AttributesInUpdateWriteOnlyChannelRequest Attributes
        {
            get { return BackingStore?.Get<OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.Models.AttributesInUpdateWriteOnlyChannelRequest>("attributes"); }
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
        /// <summary>The relationships property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.Models.RelationshipsInUpdateWriteOnlyChannelRequest? Relationships
        {
            get { return BackingStore?.Get<OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.Models.RelationshipsInUpdateWriteOnlyChannelRequest?>("relationships"); }
            set { BackingStore?.Set("relationships", value); }
        }
#nullable restore
#else
        public OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.Models.RelationshipsInUpdateWriteOnlyChannelRequest Relationships
        {
            get { return BackingStore?.Get<OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.Models.RelationshipsInUpdateWriteOnlyChannelRequest>("relationships"); }
            set { BackingStore?.Set("relationships", value); }
        }
#endif
        /// <summary>The type property</summary>
        public OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.Models.WriteOnlyChannelResourceType? Type
        {
            get { return BackingStore?.Get<OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.Models.WriteOnlyChannelResourceType?>("type"); }
            set { BackingStore?.Set("type", value); }
        }
        /// <summary>
        /// Instantiates a new <see cref="OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.Models.DataInUpdateWriteOnlyChannelRequest"/> and sets the default values.
        /// </summary>
        public DataInUpdateWriteOnlyChannelRequest()
        {
            BackingStore = BackingStoreFactorySingleton.Instance.CreateBackingStore();
        }
        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// </summary>
        /// <returns>A <see cref="OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.Models.DataInUpdateWriteOnlyChannelRequest"/></returns>
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        public static OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.Models.DataInUpdateWriteOnlyChannelRequest CreateFromDiscriminatorValue(IParseNode parseNode)
        {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            return new OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.Models.DataInUpdateWriteOnlyChannelRequest();
        }
        /// <summary>
        /// The deserialization information for the current model
        /// </summary>
        /// <returns>A IDictionary&lt;string, Action&lt;IParseNode&gt;&gt;</returns>
        public virtual IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
        {
            return new Dictionary<string, Action<IParseNode>>
            {
                { "attributes", n => { Attributes = n.GetObjectValue<OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.Models.AttributesInUpdateWriteOnlyChannelRequest>(OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.Models.AttributesInUpdateWriteOnlyChannelRequest.CreateFromDiscriminatorValue); } },
                { "id", n => { Id = n.GetStringValue(); } },
                { "relationships", n => { Relationships = n.GetObjectValue<OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.Models.RelationshipsInUpdateWriteOnlyChannelRequest>(OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.Models.RelationshipsInUpdateWriteOnlyChannelRequest.CreateFromDiscriminatorValue); } },
                { "type", n => { Type = n.GetEnumValue<OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.Models.WriteOnlyChannelResourceType>(); } },
            };
        }
        /// <summary>
        /// Serializes information the current object
        /// </summary>
        /// <param name="writer">Serialization writer to use to serialize this model</param>
        public virtual void Serialize(ISerializationWriter writer)
        {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            writer.WriteObjectValue<OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.Models.AttributesInUpdateWriteOnlyChannelRequest>("attributes", Attributes);
            writer.WriteStringValue("id", Id);
            writer.WriteObjectValue<OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.Models.RelationshipsInUpdateWriteOnlyChannelRequest>("relationships", Relationships);
            writer.WriteEnumValue<OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.Models.WriteOnlyChannelResourceType>("type", Type);
        }
    }
}