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
    public class TeacherIdentifierCollectionResponseDocument : IBackedModel, IParsable
    #pragma warning restore CS1591
    {
        /// <summary>Stores model information.</summary>
        public IBackingStore BackingStore { get; private set; }
        /// <summary>The data property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public List<OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.TeacherIdentifierInResponse>? Data
        {
            get { return BackingStore?.Get<List<OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.TeacherIdentifierInResponse>?>("data"); }
            set { BackingStore?.Set("data", value); }
        }
#nullable restore
#else
        public List<OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.TeacherIdentifierInResponse> Data
        {
            get { return BackingStore?.Get<List<OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.TeacherIdentifierInResponse>>("data"); }
            set { BackingStore?.Set("data", value); }
        }
#endif
        /// <summary>The jsonapi property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.Jsonapi? Jsonapi
        {
            get { return BackingStore?.Get<OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.Jsonapi?>("jsonapi"); }
            set { BackingStore?.Set("jsonapi", value); }
        }
#nullable restore
#else
        public OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.Jsonapi Jsonapi
        {
            get { return BackingStore?.Get<OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.Jsonapi>("jsonapi"); }
            set { BackingStore?.Set("jsonapi", value); }
        }
#endif
        /// <summary>The links property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.ResourceIdentifierCollectionTopLevelLinks? Links
        {
            get { return BackingStore?.Get<OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.ResourceIdentifierCollectionTopLevelLinks?>("links"); }
            set { BackingStore?.Set("links", value); }
        }
#nullable restore
#else
        public OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.ResourceIdentifierCollectionTopLevelLinks Links
        {
            get { return BackingStore?.Get<OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.ResourceIdentifierCollectionTopLevelLinks>("links"); }
            set { BackingStore?.Set("links", value); }
        }
#endif
        /// <summary>The meta property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.Meta? Meta
        {
            get { return BackingStore?.Get<OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.Meta?>("meta"); }
            set { BackingStore?.Set("meta", value); }
        }
#nullable restore
#else
        public OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.Meta Meta
        {
            get { return BackingStore?.Get<OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.Meta>("meta"); }
            set { BackingStore?.Set("meta", value); }
        }
#endif
        /// <summary>
        /// Instantiates a new <see cref="OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.TeacherIdentifierCollectionResponseDocument"/> and sets the default values.
        /// </summary>
        public TeacherIdentifierCollectionResponseDocument()
        {
            BackingStore = BackingStoreFactorySingleton.Instance.CreateBackingStore();
        }
        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// </summary>
        /// <returns>A <see cref="OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.TeacherIdentifierCollectionResponseDocument"/></returns>
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        public static OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.TeacherIdentifierCollectionResponseDocument CreateFromDiscriminatorValue(IParseNode parseNode)
        {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            return new OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.TeacherIdentifierCollectionResponseDocument();
        }
        /// <summary>
        /// The deserialization information for the current model
        /// </summary>
        /// <returns>A IDictionary&lt;string, Action&lt;IParseNode&gt;&gt;</returns>
        public virtual IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
        {
            return new Dictionary<string, Action<IParseNode>>
            {
                { "data", n => { Data = n.GetCollectionOfObjectValues<OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.TeacherIdentifierInResponse>(OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.TeacherIdentifierInResponse.CreateFromDiscriminatorValue)?.ToList(); } },
                { "jsonapi", n => { Jsonapi = n.GetObjectValue<OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.Jsonapi>(OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.Jsonapi.CreateFromDiscriminatorValue); } },
                { "links", n => { Links = n.GetObjectValue<OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.ResourceIdentifierCollectionTopLevelLinks>(OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.ResourceIdentifierCollectionTopLevelLinks.CreateFromDiscriminatorValue); } },
                { "meta", n => { Meta = n.GetObjectValue<OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.Meta>(OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.Meta.CreateFromDiscriminatorValue); } },
            };
        }
        /// <summary>
        /// Serializes information the current object
        /// </summary>
        /// <param name="writer">Serialization writer to use to serialize this model</param>
        public virtual void Serialize(ISerializationWriter writer)
        {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            writer.WriteCollectionOfObjectValues<OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.TeacherIdentifierInResponse>("data", Data);
            writer.WriteObjectValue<OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.Jsonapi>("jsonapi", Jsonapi);
            writer.WriteObjectValue<OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.ResourceIdentifierCollectionTopLevelLinks>("links", Links);
            writer.WriteObjectValue<OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.Meta>("meta", Meta);
        }
    }
}