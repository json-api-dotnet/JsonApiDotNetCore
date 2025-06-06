// <auto-generated/>
#nullable enable
#pragma warning disable CS8625
#pragma warning disable CS0618
using Microsoft.Kiota.Abstractions.Extensions;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions.Store;
using System.Collections.Generic;
using System.IO;
using System;
namespace OpenApiKiotaEndToEndTests.ClientIdGenerationModes.GeneratedCode.Models
{
    [global::System.CodeDom.Compiler.GeneratedCode("Kiota", "1.0.0")]
    #pragma warning disable CS1591
    public partial class ToManyPlayerInRequest : IBackedModel, IParsable
    #pragma warning restore CS1591
    {
        /// <summary>Stores model information.</summary>
        public IBackingStore BackingStore { get; private set; }

        /// <summary>The data property</summary>
        public List<global::OpenApiKiotaEndToEndTests.ClientIdGenerationModes.GeneratedCode.Models.PlayerIdentifierInRequest>? Data
        {
            get { return BackingStore?.Get<List<global::OpenApiKiotaEndToEndTests.ClientIdGenerationModes.GeneratedCode.Models.PlayerIdentifierInRequest>?>("data"); }
            set { BackingStore?.Set("data", value); }
        }

        /// <summary>The meta property</summary>
        public global::OpenApiKiotaEndToEndTests.ClientIdGenerationModes.GeneratedCode.Models.Meta? Meta
        {
            get { return BackingStore?.Get<global::OpenApiKiotaEndToEndTests.ClientIdGenerationModes.GeneratedCode.Models.Meta?>("meta"); }
            set { BackingStore?.Set("meta", value); }
        }

        /// <summary>
        /// Instantiates a new <see cref="global::OpenApiKiotaEndToEndTests.ClientIdGenerationModes.GeneratedCode.Models.ToManyPlayerInRequest"/> and sets the default values.
        /// </summary>
        public ToManyPlayerInRequest()
        {
            BackingStore = BackingStoreFactorySingleton.Instance.CreateBackingStore();
        }

        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// </summary>
        /// <returns>A <see cref="global::OpenApiKiotaEndToEndTests.ClientIdGenerationModes.GeneratedCode.Models.ToManyPlayerInRequest"/></returns>
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        public static global::OpenApiKiotaEndToEndTests.ClientIdGenerationModes.GeneratedCode.Models.ToManyPlayerInRequest CreateFromDiscriminatorValue(IParseNode parseNode)
        {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            return new global::OpenApiKiotaEndToEndTests.ClientIdGenerationModes.GeneratedCode.Models.ToManyPlayerInRequest();
        }

        /// <summary>
        /// The deserialization information for the current model
        /// </summary>
        /// <returns>A IDictionary&lt;string, Action&lt;IParseNode&gt;&gt;</returns>
        public virtual IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
        {
            return new Dictionary<string, Action<IParseNode>>
            {
                { "data", n => { Data = n.GetCollectionOfObjectValues<global::OpenApiKiotaEndToEndTests.ClientIdGenerationModes.GeneratedCode.Models.PlayerIdentifierInRequest>(global::OpenApiKiotaEndToEndTests.ClientIdGenerationModes.GeneratedCode.Models.PlayerIdentifierInRequest.CreateFromDiscriminatorValue)?.AsList(); } },
                { "meta", n => { Meta = n.GetObjectValue<global::OpenApiKiotaEndToEndTests.ClientIdGenerationModes.GeneratedCode.Models.Meta>(global::OpenApiKiotaEndToEndTests.ClientIdGenerationModes.GeneratedCode.Models.Meta.CreateFromDiscriminatorValue); } },
            };
        }

        /// <summary>
        /// Serializes information the current object
        /// </summary>
        /// <param name="writer">Serialization writer to use to serialize this model</param>
        public virtual void Serialize(ISerializationWriter writer)
        {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            writer.WriteCollectionOfObjectValues<global::OpenApiKiotaEndToEndTests.ClientIdGenerationModes.GeneratedCode.Models.PlayerIdentifierInRequest>("data", Data);
            writer.WriteObjectValue<global::OpenApiKiotaEndToEndTests.ClientIdGenerationModes.GeneratedCode.Models.Meta>("meta", Meta);
        }
    }
}
#pragma warning restore CS0618
