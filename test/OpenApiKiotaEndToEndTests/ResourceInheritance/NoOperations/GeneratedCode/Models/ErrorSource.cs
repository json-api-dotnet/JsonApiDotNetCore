// <auto-generated/>
#pragma warning disable CS0618
using Microsoft.Kiota.Abstractions.Extensions;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions.Store;
using System.Collections.Generic;
using System.IO;
using System;
namespace OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Models
{
    [global::System.CodeDom.Compiler.GeneratedCode("Kiota", "1.0.0")]
    #pragma warning disable CS1591
    public partial class ErrorSource : IBackedModel, IParsable
    #pragma warning restore CS1591
    {
        /// <summary>Stores model information.</summary>
        public IBackingStore BackingStore { get; private set; }
        /// <summary>The header property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public string? Header
        {
            get { return BackingStore?.Get<string?>("header"); }
            set { BackingStore?.Set("header", value); }
        }
#nullable restore
#else
        public string Header
        {
            get { return BackingStore?.Get<string>("header"); }
            set { BackingStore?.Set("header", value); }
        }
#endif
        /// <summary>The parameter property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public string? Parameter
        {
            get { return BackingStore?.Get<string?>("parameter"); }
            set { BackingStore?.Set("parameter", value); }
        }
#nullable restore
#else
        public string Parameter
        {
            get { return BackingStore?.Get<string>("parameter"); }
            set { BackingStore?.Set("parameter", value); }
        }
#endif
        /// <summary>The pointer property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public string? Pointer
        {
            get { return BackingStore?.Get<string?>("pointer"); }
            set { BackingStore?.Set("pointer", value); }
        }
#nullable restore
#else
        public string Pointer
        {
            get { return BackingStore?.Get<string>("pointer"); }
            set { BackingStore?.Set("pointer", value); }
        }
#endif
        /// <summary>
        /// Instantiates a new <see cref="global::OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Models.ErrorSource"/> and sets the default values.
        /// </summary>
        public ErrorSource()
        {
            BackingStore = BackingStoreFactorySingleton.Instance.CreateBackingStore();
        }
        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// </summary>
        /// <returns>A <see cref="global::OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Models.ErrorSource"/></returns>
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        public static global::OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Models.ErrorSource CreateFromDiscriminatorValue(IParseNode parseNode)
        {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            return new global::OpenApiKiotaEndToEndTests.ResourceInheritance.NoOperations.GeneratedCode.Models.ErrorSource();
        }
        /// <summary>
        /// The deserialization information for the current model
        /// </summary>
        /// <returns>A IDictionary&lt;string, Action&lt;IParseNode&gt;&gt;</returns>
        public virtual IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
        {
            return new Dictionary<string, Action<IParseNode>>
            {
                { "header", n => { Header = n.GetStringValue(); } },
                { "parameter", n => { Parameter = n.GetStringValue(); } },
                { "pointer", n => { Pointer = n.GetStringValue(); } },
            };
        }
        /// <summary>
        /// Serializes information the current object
        /// </summary>
        /// <param name="writer">Serialization writer to use to serialize this model</param>
        public virtual void Serialize(ISerializationWriter writer)
        {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            writer.WriteStringValue("header", Header);
            writer.WriteStringValue("parameter", Parameter);
            writer.WriteStringValue("pointer", Pointer);
        }
    }
}
#pragma warning restore CS0618