// <auto-generated/>
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions.Store;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
namespace OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models {
    public class VacationPostRequestDocument : IBackedModel, IParsable {
        /// <summary>Stores model information.</summary>
        public IBackingStore BackingStore { get; private set; }
        /// <summary>The data property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public VacationDataInPostRequest? Data {
            get { return BackingStore?.Get<VacationDataInPostRequest?>("data"); }
            set { BackingStore?.Set("data", value); }
        }
#nullable restore
#else
        public VacationDataInPostRequest Data {
            get { return BackingStore?.Get<VacationDataInPostRequest>("data"); }
            set { BackingStore?.Set("data", value); }
        }
#endif
        /// <summary>
        /// Instantiates a new vacationPostRequestDocument and sets the default values.
        /// </summary>
        public VacationPostRequestDocument() {
            BackingStore = BackingStoreFactorySingleton.Instance.CreateBackingStore();
        }
        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// </summary>
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        public static VacationPostRequestDocument CreateFromDiscriminatorValue(IParseNode parseNode) {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            return new VacationPostRequestDocument();
        }
        /// <summary>
        /// The deserialization information for the current model
        /// </summary>
        public virtual IDictionary<string, Action<IParseNode>> GetFieldDeserializers() {
            return new Dictionary<string, Action<IParseNode>> {
                {"data", n => { Data = n.GetObjectValue<VacationDataInPostRequest>(VacationDataInPostRequest.CreateFromDiscriminatorValue); } },
            };
        }
        /// <summary>
        /// Serializes information the current object
        /// </summary>
        /// <param name="writer">Serialization writer to use to serialize this model</param>
        public virtual void Serialize(ISerializationWriter writer) {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            writer.WriteObjectValue<VacationDataInPostRequest>("data", Data);
        }
    }
}
