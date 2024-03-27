// <auto-generated/>
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions.Store;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
namespace OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models {
    public class TransportDataInPostRequest : IBackedModel, IParsable {
        /// <summary>The attributes property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public TransportAttributesInPostRequest? Attributes {
            get { return BackingStore?.Get<TransportAttributesInPostRequest?>("attributes"); }
            set { BackingStore?.Set("attributes", value); }
        }
#nullable restore
#else
        public TransportAttributesInPostRequest Attributes {
            get { return BackingStore?.Get<TransportAttributesInPostRequest>("attributes"); }
            set { BackingStore?.Set("attributes", value); }
        }
#endif
        /// <summary>Stores model information.</summary>
        public IBackingStore BackingStore { get; private set; }
        /// <summary>The type property</summary>
        public TransportResourceType? Type {
            get { return BackingStore?.Get<TransportResourceType?>("type"); }
            set { BackingStore?.Set("type", value); }
        }
        /// <summary>
        /// Instantiates a new transportDataInPostRequest and sets the default values.
        /// </summary>
        public TransportDataInPostRequest() {
            BackingStore = BackingStoreFactorySingleton.Instance.CreateBackingStore();
        }
        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// </summary>
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        public static TransportDataInPostRequest CreateFromDiscriminatorValue(IParseNode parseNode) {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            return new TransportDataInPostRequest();
        }
        /// <summary>
        /// The deserialization information for the current model
        /// </summary>
        public virtual IDictionary<string, Action<IParseNode>> GetFieldDeserializers() {
            return new Dictionary<string, Action<IParseNode>> {
                {"attributes", n => { Attributes = n.GetObjectValue<TransportAttributesInPostRequest>(TransportAttributesInPostRequest.CreateFromDiscriminatorValue); } },
                {"type", n => { Type = n.GetEnumValue<TransportResourceType>(); } },
            };
        }
        /// <summary>
        /// Serializes information the current object
        /// </summary>
        /// <param name="writer">Serialization writer to use to serialize this model</param>
        public virtual void Serialize(ISerializationWriter writer) {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            writer.WriteObjectValue<TransportAttributesInPostRequest>("attributes", Attributes);
            writer.WriteEnumValue<TransportResourceType>("type", Type);
        }
    }
}
