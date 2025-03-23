// <auto-generated/>
#nullable enable
#pragma warning disable CS8625
#pragma warning disable CS0618
using Microsoft.Kiota.Abstractions.Extensions;
using Microsoft.Kiota.Abstractions.Serialization;
using System.Collections.Generic;
using System.IO;
using System;
namespace OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.Models
{
    [global::System.CodeDom.Compiler.GeneratedCode("Kiota", "1.0.0")]
    #pragma warning disable CS1591
    public partial class AttributesInUpdateWriteOnlyChannelRequest : global::OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.Models.AttributesInUpdateRequest, IParsable
    #pragma warning restore CS1591
    {
        /// <summary>The isAdultOnly property</summary>
        public bool? IsAdultOnly
        {
            get { return BackingStore?.Get<bool?>("isAdultOnly"); }
            set { BackingStore?.Set("isAdultOnly", value); }
        }

        /// <summary>The isCommercial property</summary>
        public bool? IsCommercial
        {
            get { return BackingStore?.Get<bool?>("isCommercial"); }
            set { BackingStore?.Set("isCommercial", value); }
        }

        /// <summary>The name property</summary>
        public string? Name
        {
            get { return BackingStore?.Get<string?>("name"); }
            set { BackingStore?.Set("name", value); }
        }

        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// </summary>
        /// <returns>A <see cref="global::OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.Models.AttributesInUpdateWriteOnlyChannelRequest"/></returns>
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        public static new global::OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.Models.AttributesInUpdateWriteOnlyChannelRequest CreateFromDiscriminatorValue(IParseNode parseNode)
        {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            return new global::OpenApiKiotaEndToEndTests.RestrictedControllers.GeneratedCode.Models.AttributesInUpdateWriteOnlyChannelRequest();
        }

        /// <summary>
        /// The deserialization information for the current model
        /// </summary>
        /// <returns>A IDictionary&lt;string, Action&lt;IParseNode&gt;&gt;</returns>
        public override IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
        {
            return new Dictionary<string, Action<IParseNode>>(base.GetFieldDeserializers())
            {
                { "isAdultOnly", n => { IsAdultOnly = n.GetBoolValue(); } },
                { "isCommercial", n => { IsCommercial = n.GetBoolValue(); } },
                { "name", n => { Name = n.GetStringValue(); } },
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
            writer.WriteBoolValue("isAdultOnly", IsAdultOnly);
            writer.WriteBoolValue("isCommercial", IsCommercial);
            writer.WriteStringValue("name", Name);
        }
    }
}
#pragma warning restore CS0618
