// <auto-generated/>
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions.Store;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
namespace OpenApiKiotaEndToEndTests.ClientIdGenerationModes.GeneratedCode.Models
{
    #pragma warning disable CS1591
    public class GameIdentifierInResponse : IBackedModel, IParsable
    #pragma warning restore CS1591
    {
        /// <summary>Stores model information.</summary>
        public IBackingStore BackingStore { get; private set; }
        /// <summary>The id property</summary>
        public Guid? Id
        {
            get { return BackingStore?.Get<Guid?>("id"); }
            set { BackingStore?.Set("id", value); }
        }
        /// <summary>The type property</summary>
        public OpenApiKiotaEndToEndTests.ClientIdGenerationModes.GeneratedCode.Models.GameResourceType? Type
        {
            get { return BackingStore?.Get<OpenApiKiotaEndToEndTests.ClientIdGenerationModes.GeneratedCode.Models.GameResourceType?>("type"); }
            set { BackingStore?.Set("type", value); }
        }
        /// <summary>
        /// Instantiates a new <see cref="OpenApiKiotaEndToEndTests.ClientIdGenerationModes.GeneratedCode.Models.GameIdentifierInResponse"/> and sets the default values.
        /// </summary>
        public GameIdentifierInResponse()
        {
            BackingStore = BackingStoreFactorySingleton.Instance.CreateBackingStore();
        }
        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// </summary>
        /// <returns>A <see cref="OpenApiKiotaEndToEndTests.ClientIdGenerationModes.GeneratedCode.Models.GameIdentifierInResponse"/></returns>
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        public static OpenApiKiotaEndToEndTests.ClientIdGenerationModes.GeneratedCode.Models.GameIdentifierInResponse CreateFromDiscriminatorValue(IParseNode parseNode)
        {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            return new OpenApiKiotaEndToEndTests.ClientIdGenerationModes.GeneratedCode.Models.GameIdentifierInResponse();
        }
        /// <summary>
        /// The deserialization information for the current model
        /// </summary>
        /// <returns>A IDictionary&lt;string, Action&lt;IParseNode&gt;&gt;</returns>
        public virtual IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
        {
            return new Dictionary<string, Action<IParseNode>>
            {
                { "id", n => { Id = n.GetGuidValue(); } },
                { "type", n => { Type = n.GetEnumValue<OpenApiKiotaEndToEndTests.ClientIdGenerationModes.GeneratedCode.Models.GameResourceType>(); } },
            };
        }
        /// <summary>
        /// Serializes information the current object
        /// </summary>
        /// <param name="writer">Serialization writer to use to serialize this model</param>
        public virtual void Serialize(ISerializationWriter writer)
        {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            writer.WriteGuidValue("id", Id);
            writer.WriteEnumValue<OpenApiKiotaEndToEndTests.ClientIdGenerationModes.GeneratedCode.Models.GameResourceType>("type", Type);
        }
    }
}
