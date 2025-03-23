// <auto-generated/>
#nullable enable
#pragma warning disable CS8625
#pragma warning disable CS0618
using Microsoft.Kiota.Abstractions.Extensions;
using Microsoft.Kiota.Abstractions.Serialization;
using System.Collections.Generic;
using System.IO;
using System;
namespace OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models
{
    [global::System.CodeDom.Compiler.GeneratedCode("Kiota", "1.0.0")]
    #pragma warning disable CS1591
    public partial class AttributesInTransportResponse : global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.AttributesInResponse, IParsable
    #pragma warning restore CS1591
    {
        /// <summary>The durationInMinutes property</summary>
        public int? DurationInMinutes
        {
            get { return BackingStore?.Get<int?>("durationInMinutes"); }
            set { BackingStore?.Set("durationInMinutes", value); }
        }

        /// <summary>The type property</summary>
        public int? Type
        {
            get { return BackingStore?.Get<int?>("type"); }
            set { BackingStore?.Set("type", value); }
        }

        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// </summary>
        /// <returns>A <see cref="global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.AttributesInTransportResponse"/></returns>
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        public static new global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.AttributesInTransportResponse CreateFromDiscriminatorValue(IParseNode parseNode)
        {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            return new global::OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.AttributesInTransportResponse();
        }

        /// <summary>
        /// The deserialization information for the current model
        /// </summary>
        /// <returns>A IDictionary&lt;string, Action&lt;IParseNode&gt;&gt;</returns>
        public override IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
        {
            return new Dictionary<string, Action<IParseNode>>(base.GetFieldDeserializers())
            {
                { "durationInMinutes", n => { DurationInMinutes = n.GetIntValue(); } },
                { "type", n => { Type = n.GetIntValue(); } },
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
            writer.WriteIntValue("durationInMinutes", DurationInMinutes);
            writer.WriteIntValue("type", Type);
        }
    }
}
#pragma warning restore CS0618
