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
namespace OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models
{
    [global::System.CodeDom.Compiler.GeneratedCode("Kiota", "1.0.0")]
    #pragma warning disable CS1591
    public partial class OperationsResponseDocument : IBackedModel, IParsable
    #pragma warning restore CS1591
    {
        /// <summary>The atomicResults property</summary>
        public List<global::OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.AtomicResult>? AtomicResults
        {
            get { return BackingStore?.Get<List<global::OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.AtomicResult>?>("atomic:results"); }
            set { BackingStore?.Set("atomic:results", value); }
        }

        /// <summary>Stores model information.</summary>
        public IBackingStore BackingStore { get; private set; }

        /// <summary>The jsonapi property</summary>
        public global::OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.Jsonapi? Jsonapi
        {
            get { return BackingStore?.Get<global::OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.Jsonapi?>("jsonapi"); }
            set { BackingStore?.Set("jsonapi", value); }
        }

        /// <summary>The links property</summary>
        public global::OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.ResourceTopLevelLinks? Links
        {
            get { return BackingStore?.Get<global::OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.ResourceTopLevelLinks?>("links"); }
            set { BackingStore?.Set("links", value); }
        }

        /// <summary>The meta property</summary>
        public global::OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.Meta? Meta
        {
            get { return BackingStore?.Get<global::OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.Meta?>("meta"); }
            set { BackingStore?.Set("meta", value); }
        }

        /// <summary>
        /// Instantiates a new <see cref="global::OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.OperationsResponseDocument"/> and sets the default values.
        /// </summary>
        public OperationsResponseDocument()
        {
            BackingStore = BackingStoreFactorySingleton.Instance.CreateBackingStore();
        }

        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// </summary>
        /// <returns>A <see cref="global::OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.OperationsResponseDocument"/></returns>
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        public static global::OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.OperationsResponseDocument CreateFromDiscriminatorValue(IParseNode parseNode)
        {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            return new global::OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.OperationsResponseDocument();
        }

        /// <summary>
        /// The deserialization information for the current model
        /// </summary>
        /// <returns>A IDictionary&lt;string, Action&lt;IParseNode&gt;&gt;</returns>
        public virtual IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
        {
            return new Dictionary<string, Action<IParseNode>>
            {
                { "atomic:results", n => { AtomicResults = n.GetCollectionOfObjectValues<global::OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.AtomicResult>(global::OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.AtomicResult.CreateFromDiscriminatorValue)?.AsList(); } },
                { "jsonapi", n => { Jsonapi = n.GetObjectValue<global::OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.Jsonapi>(global::OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.Jsonapi.CreateFromDiscriminatorValue); } },
                { "links", n => { Links = n.GetObjectValue<global::OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.ResourceTopLevelLinks>(global::OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.ResourceTopLevelLinks.CreateFromDiscriminatorValue); } },
                { "meta", n => { Meta = n.GetObjectValue<global::OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.Meta>(global::OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.Meta.CreateFromDiscriminatorValue); } },
            };
        }

        /// <summary>
        /// Serializes information the current object
        /// </summary>
        /// <param name="writer">Serialization writer to use to serialize this model</param>
        public virtual void Serialize(ISerializationWriter writer)
        {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            writer.WriteCollectionOfObjectValues<global::OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.AtomicResult>("atomic:results", AtomicResults);
            writer.WriteObjectValue<global::OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.Jsonapi>("jsonapi", Jsonapi);
            writer.WriteObjectValue<global::OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.ResourceTopLevelLinks>("links", Links);
            writer.WriteObjectValue<global::OpenApiKiotaEndToEndTests.AtomicOperations.GeneratedCode.Models.Meta>("meta", Meta);
        }
    }
}
#pragma warning restore CS0618
