// <auto-generated/>
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions.Store;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
namespace OpenApiKiotaClientExample.GeneratedCode.Models {
    #pragma warning disable CS1591
    public class TagDataInPostRequest : IBackedModel, IParsable 
    #pragma warning restore CS1591
    {
        /// <summary>The attributes property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public TagAttributesInPostRequest? Attributes {
            get { return BackingStore?.Get<TagAttributesInPostRequest?>("attributes"); }
            set { BackingStore?.Set("attributes", value); }
        }
#nullable restore
#else
        public TagAttributesInPostRequest Attributes {
            get { return BackingStore?.Get<TagAttributesInPostRequest>("attributes"); }
            set { BackingStore?.Set("attributes", value); }
        }
#endif
        /// <summary>Stores model information.</summary>
        public IBackingStore BackingStore { get; private set; }
        /// <summary>The relationships property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public TagRelationshipsInPostRequest? Relationships {
            get { return BackingStore?.Get<TagRelationshipsInPostRequest?>("relationships"); }
            set { BackingStore?.Set("relationships", value); }
        }
#nullable restore
#else
        public TagRelationshipsInPostRequest Relationships {
            get { return BackingStore?.Get<TagRelationshipsInPostRequest>("relationships"); }
            set { BackingStore?.Set("relationships", value); }
        }
#endif
        /// <summary>The type property</summary>
        public TagResourceType? Type {
            get { return BackingStore?.Get<TagResourceType?>("type"); }
            set { BackingStore?.Set("type", value); }
        }
        /// <summary>
        /// Instantiates a new <see cref="TagDataInPostRequest"/> and sets the default values.
        /// </summary>
        public TagDataInPostRequest()
        {
            BackingStore = BackingStoreFactorySingleton.Instance.CreateBackingStore();
        }
        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// </summary>
        /// <returns>A <see cref="TagDataInPostRequest"/></returns>
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        public static TagDataInPostRequest CreateFromDiscriminatorValue(IParseNode parseNode)
        {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            return new TagDataInPostRequest();
        }
        /// <summary>
        /// The deserialization information for the current model
        /// </summary>
        /// <returns>A IDictionary&lt;string, Action&lt;IParseNode&gt;&gt;</returns>
        public virtual IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
        {
            return new Dictionary<string, Action<IParseNode>>
            {
                {"attributes", n => { Attributes = n.GetObjectValue<TagAttributesInPostRequest>(TagAttributesInPostRequest.CreateFromDiscriminatorValue); } },
                {"relationships", n => { Relationships = n.GetObjectValue<TagRelationshipsInPostRequest>(TagRelationshipsInPostRequest.CreateFromDiscriminatorValue); } },
                {"type", n => { Type = n.GetEnumValue<TagResourceType>(); } },
            };
        }
        /// <summary>
        /// Serializes information the current object
        /// </summary>
        /// <param name="writer">Serialization writer to use to serialize this model</param>
        public virtual void Serialize(ISerializationWriter writer)
        {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            writer.WriteObjectValue<TagAttributesInPostRequest>("attributes", Attributes);
            writer.WriteObjectValue<TagRelationshipsInPostRequest>("relationships", Relationships);
            writer.WriteEnumValue<TagResourceType>("type", Type);
        }
    }
}