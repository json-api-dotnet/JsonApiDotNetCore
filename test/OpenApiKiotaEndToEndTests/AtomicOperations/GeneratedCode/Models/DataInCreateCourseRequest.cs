// <auto-generated/>
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions.Store;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
namespace OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models
{
    #pragma warning disable CS1591
    public class DataInCreateCourseRequest : IBackedModel, IParsable
    #pragma warning restore CS1591
    {
        /// <summary>The attributes property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.AttributesInCreateCourseRequest? Attributes
        {
            get { return BackingStore?.Get<OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.AttributesInCreateCourseRequest?>("attributes"); }
            set { BackingStore?.Set("attributes", value); }
        }
#nullable restore
#else
        public OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.AttributesInCreateCourseRequest Attributes
        {
            get { return BackingStore?.Get<OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.AttributesInCreateCourseRequest>("attributes"); }
            set { BackingStore?.Set("attributes", value); }
        }
#endif
        /// <summary>Stores model information.</summary>
        public IBackingStore BackingStore { get; private set; }
        /// <summary>The id property</summary>
        public Guid? Id
        {
            get { return BackingStore?.Get<Guid?>("id"); }
            set { BackingStore?.Set("id", value); }
        }
        /// <summary>The relationships property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.RelationshipsInCreateCourseRequest? Relationships
        {
            get { return BackingStore?.Get<OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.RelationshipsInCreateCourseRequest?>("relationships"); }
            set { BackingStore?.Set("relationships", value); }
        }
#nullable restore
#else
        public OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.RelationshipsInCreateCourseRequest Relationships
        {
            get { return BackingStore?.Get<OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.RelationshipsInCreateCourseRequest>("relationships"); }
            set { BackingStore?.Set("relationships", value); }
        }
#endif
        /// <summary>The type property</summary>
        public OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.CourseResourceType? Type
        {
            get { return BackingStore?.Get<OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.CourseResourceType?>("type"); }
            set { BackingStore?.Set("type", value); }
        }
        /// <summary>
        /// Instantiates a new <see cref="OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.DataInCreateCourseRequest"/> and sets the default values.
        /// </summary>
        public DataInCreateCourseRequest()
        {
            BackingStore = BackingStoreFactorySingleton.Instance.CreateBackingStore();
        }
        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// </summary>
        /// <returns>A <see cref="OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.DataInCreateCourseRequest"/></returns>
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        public static OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.DataInCreateCourseRequest CreateFromDiscriminatorValue(IParseNode parseNode)
        {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            return new OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.DataInCreateCourseRequest();
        }
        /// <summary>
        /// The deserialization information for the current model
        /// </summary>
        /// <returns>A IDictionary&lt;string, Action&lt;IParseNode&gt;&gt;</returns>
        public virtual IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
        {
            return new Dictionary<string, Action<IParseNode>>
            {
                { "attributes", n => { Attributes = n.GetObjectValue<OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.AttributesInCreateCourseRequest>(OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.AttributesInCreateCourseRequest.CreateFromDiscriminatorValue); } },
                { "id", n => { Id = n.GetGuidValue(); } },
                { "relationships", n => { Relationships = n.GetObjectValue<OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.RelationshipsInCreateCourseRequest>(OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.RelationshipsInCreateCourseRequest.CreateFromDiscriminatorValue); } },
                { "type", n => { Type = n.GetEnumValue<OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.CourseResourceType>(); } },
            };
        }
        /// <summary>
        /// Serializes information the current object
        /// </summary>
        /// <param name="writer">Serialization writer to use to serialize this model</param>
        public virtual void Serialize(ISerializationWriter writer)
        {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            writer.WriteObjectValue<OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.AttributesInCreateCourseRequest>("attributes", Attributes);
            writer.WriteGuidValue("id", Id);
            writer.WriteObjectValue<OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.RelationshipsInCreateCourseRequest>("relationships", Relationships);
            writer.WriteEnumValue<OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.CourseResourceType>("type", Type);
        }
    }
}