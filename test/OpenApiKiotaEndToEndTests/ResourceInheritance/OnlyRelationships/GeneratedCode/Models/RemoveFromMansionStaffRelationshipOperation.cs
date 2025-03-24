// <auto-generated/>
#nullable enable
#pragma warning disable CS8625
#pragma warning disable CS0618
using Microsoft.Kiota.Abstractions.Extensions;
using Microsoft.Kiota.Abstractions.Serialization;
using System.Collections.Generic;
using System.IO;
using System;
namespace OpenApiKiotaEndToEndTests.ResourceInheritance.OnlyRelationships.GeneratedCode.Models
{
    [global::System.CodeDom.Compiler.GeneratedCode("Kiota", "1.0.0")]
    #pragma warning disable CS1591
    public partial class RemoveFromMansionStaffRelationshipOperation : global::OpenApiKiotaEndToEndTests.ResourceInheritance.OnlyRelationships.GeneratedCode.Models.AtomicOperation, IParsable
    #pragma warning restore CS1591
    {
        /// <summary>The data property</summary>
        public List<global::OpenApiKiotaEndToEndTests.ResourceInheritance.OnlyRelationships.GeneratedCode.Models.StaffMemberIdentifierInRequest>? Data
        {
            get { return BackingStore?.Get<List<global::OpenApiKiotaEndToEndTests.ResourceInheritance.OnlyRelationships.GeneratedCode.Models.StaffMemberIdentifierInRequest>?>("data"); }
            set { BackingStore?.Set("data", value); }
        }

        /// <summary>The op property</summary>
        public global::OpenApiKiotaEndToEndTests.ResourceInheritance.OnlyRelationships.GeneratedCode.Models.RemoveOperationCode? Op
        {
            get { return BackingStore?.Get<global::OpenApiKiotaEndToEndTests.ResourceInheritance.OnlyRelationships.GeneratedCode.Models.RemoveOperationCode?>("op"); }
            set { BackingStore?.Set("op", value); }
        }

        /// <summary>The ref property</summary>
        public global::OpenApiKiotaEndToEndTests.ResourceInheritance.OnlyRelationships.GeneratedCode.Models.MansionStaffRelationshipIdentifier? Ref
        {
            get { return BackingStore?.Get<global::OpenApiKiotaEndToEndTests.ResourceInheritance.OnlyRelationships.GeneratedCode.Models.MansionStaffRelationshipIdentifier?>("ref"); }
            set { BackingStore?.Set("ref", value); }
        }

        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// </summary>
        /// <returns>A <see cref="global::OpenApiKiotaEndToEndTests.ResourceInheritance.OnlyRelationships.GeneratedCode.Models.RemoveFromMansionStaffRelationshipOperation"/></returns>
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        public static new global::OpenApiKiotaEndToEndTests.ResourceInheritance.OnlyRelationships.GeneratedCode.Models.RemoveFromMansionStaffRelationshipOperation CreateFromDiscriminatorValue(IParseNode parseNode)
        {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            return new global::OpenApiKiotaEndToEndTests.ResourceInheritance.OnlyRelationships.GeneratedCode.Models.RemoveFromMansionStaffRelationshipOperation();
        }

        /// <summary>
        /// The deserialization information for the current model
        /// </summary>
        /// <returns>A IDictionary&lt;string, Action&lt;IParseNode&gt;&gt;</returns>
        public override IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
        {
            return new Dictionary<string, Action<IParseNode>>(base.GetFieldDeserializers())
            {
                { "data", n => { Data = n.GetCollectionOfObjectValues<global::OpenApiKiotaEndToEndTests.ResourceInheritance.OnlyRelationships.GeneratedCode.Models.StaffMemberIdentifierInRequest>(global::OpenApiKiotaEndToEndTests.ResourceInheritance.OnlyRelationships.GeneratedCode.Models.StaffMemberIdentifierInRequest.CreateFromDiscriminatorValue)?.AsList(); } },
                { "op", n => { Op = n.GetEnumValue<global::OpenApiKiotaEndToEndTests.ResourceInheritance.OnlyRelationships.GeneratedCode.Models.RemoveOperationCode>(); } },
                { "ref", n => { Ref = n.GetObjectValue<global::OpenApiKiotaEndToEndTests.ResourceInheritance.OnlyRelationships.GeneratedCode.Models.MansionStaffRelationshipIdentifier>(global::OpenApiKiotaEndToEndTests.ResourceInheritance.OnlyRelationships.GeneratedCode.Models.MansionStaffRelationshipIdentifier.CreateFromDiscriminatorValue); } },
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
            writer.WriteCollectionOfObjectValues<global::OpenApiKiotaEndToEndTests.ResourceInheritance.OnlyRelationships.GeneratedCode.Models.StaffMemberIdentifierInRequest>("data", Data);
            writer.WriteEnumValue<global::OpenApiKiotaEndToEndTests.ResourceInheritance.OnlyRelationships.GeneratedCode.Models.RemoveOperationCode>("op", Op);
            writer.WriteObjectValue<global::OpenApiKiotaEndToEndTests.ResourceInheritance.OnlyRelationships.GeneratedCode.Models.MansionStaffRelationshipIdentifier>("ref", Ref);
        }
    }
}
#pragma warning restore CS0618
