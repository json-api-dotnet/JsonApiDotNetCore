// <auto-generated/>
#nullable enable
#pragma warning disable CS8625
#pragma warning disable CS0618
using Microsoft.Kiota.Abstractions.Extensions;
using Microsoft.Kiota.Abstractions.Serialization;
using System.Collections.Generic;
using System.IO;
using System;
namespace OpenApiKiotaEndToEndTests.ResourceInheritance.SubsetOfVarious.GeneratedCode.Models
{
    [global::System.CodeDom.Compiler.GeneratedCode("Kiota", "1.0.0")]
    #pragma warning disable CS1591
    public partial class RelationshipsInRoomResponse : global::OpenApiKiotaEndToEndTests.ResourceInheritance.SubsetOfVarious.GeneratedCode.Models.RelationshipsInResponse, IParsable
    #pragma warning restore CS1591
    {
        /// <summary>The residence property</summary>
        public global::OpenApiKiotaEndToEndTests.ResourceInheritance.SubsetOfVarious.GeneratedCode.Models.ToOneResidenceInResponse? Residence
        {
            get { return BackingStore?.Get<global::OpenApiKiotaEndToEndTests.ResourceInheritance.SubsetOfVarious.GeneratedCode.Models.ToOneResidenceInResponse?>("residence"); }
            set { BackingStore?.Set("residence", value); }
        }

        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// </summary>
        /// <returns>A <see cref="global::OpenApiKiotaEndToEndTests.ResourceInheritance.SubsetOfVarious.GeneratedCode.Models.RelationshipsInRoomResponse"/></returns>
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        public static new global::OpenApiKiotaEndToEndTests.ResourceInheritance.SubsetOfVarious.GeneratedCode.Models.RelationshipsInRoomResponse CreateFromDiscriminatorValue(IParseNode parseNode)
        {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            var mappingValue = parseNode.GetChildNode("openapi:discriminator")?.GetStringValue();
            return mappingValue switch
            {
                "bathrooms" => new global::OpenApiKiotaEndToEndTests.ResourceInheritance.SubsetOfVarious.GeneratedCode.Models.RelationshipsInBathroomResponse(),
                "bedrooms" => new global::OpenApiKiotaEndToEndTests.ResourceInheritance.SubsetOfVarious.GeneratedCode.Models.RelationshipsInBedroomResponse(),
                "kitchens" => new global::OpenApiKiotaEndToEndTests.ResourceInheritance.SubsetOfVarious.GeneratedCode.Models.RelationshipsInKitchenResponse(),
                "livingRooms" => new global::OpenApiKiotaEndToEndTests.ResourceInheritance.SubsetOfVarious.GeneratedCode.Models.RelationshipsInLivingRoomResponse(),
                "toilets" => new global::OpenApiKiotaEndToEndTests.ResourceInheritance.SubsetOfVarious.GeneratedCode.Models.RelationshipsInToiletResponse(),
                _ => new global::OpenApiKiotaEndToEndTests.ResourceInheritance.SubsetOfVarious.GeneratedCode.Models.RelationshipsInRoomResponse(),
            };
        }

        /// <summary>
        /// The deserialization information for the current model
        /// </summary>
        /// <returns>A IDictionary&lt;string, Action&lt;IParseNode&gt;&gt;</returns>
        public override IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
        {
            return new Dictionary<string, Action<IParseNode>>(base.GetFieldDeserializers())
            {
                { "residence", n => { Residence = n.GetObjectValue<global::OpenApiKiotaEndToEndTests.ResourceInheritance.SubsetOfVarious.GeneratedCode.Models.ToOneResidenceInResponse>(global::OpenApiKiotaEndToEndTests.ResourceInheritance.SubsetOfVarious.GeneratedCode.Models.ToOneResidenceInResponse.CreateFromDiscriminatorValue); } },
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
            writer.WriteObjectValue<global::OpenApiKiotaEndToEndTests.ResourceInheritance.SubsetOfVarious.GeneratedCode.Models.ToOneResidenceInResponse>("residence", Residence);
        }
    }
}
#pragma warning restore CS0618
