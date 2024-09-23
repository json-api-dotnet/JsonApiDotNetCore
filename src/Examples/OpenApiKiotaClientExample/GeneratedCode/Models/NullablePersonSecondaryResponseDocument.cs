// <auto-generated/>
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions.Store;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
namespace OpenApiKiotaClientExample.GeneratedCode.Models
{
    #pragma warning disable CS1591
    public class NullablePersonSecondaryResponseDocument : IBackedModel, IParsable
    #pragma warning restore CS1591
    {
        /// <summary>Stores model information.</summary>
        public IBackingStore BackingStore { get; private set; }
        /// <summary>The data property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public OpenApiKiotaClientExample.GeneratedCode.Models.PersonDataInResponse? Data
        {
            get { return BackingStore?.Get<OpenApiKiotaClientExample.GeneratedCode.Models.PersonDataInResponse?>("data"); }
            set { BackingStore?.Set("data", value); }
        }
#nullable restore
#else
        public OpenApiKiotaClientExample.GeneratedCode.Models.PersonDataInResponse Data
        {
            get { return BackingStore?.Get<OpenApiKiotaClientExample.GeneratedCode.Models.PersonDataInResponse>("data"); }
            set { BackingStore?.Set("data", value); }
        }
#endif
        /// <summary>The included property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public List<OpenApiKiotaClientExample.GeneratedCode.Models.DataInResponse>? Included
        {
            get { return BackingStore?.Get<List<OpenApiKiotaClientExample.GeneratedCode.Models.DataInResponse>?>("included"); }
            set { BackingStore?.Set("included", value); }
        }
#nullable restore
#else
        public List<OpenApiKiotaClientExample.GeneratedCode.Models.DataInResponse> Included
        {
            get { return BackingStore?.Get<List<OpenApiKiotaClientExample.GeneratedCode.Models.DataInResponse>>("included"); }
            set { BackingStore?.Set("included", value); }
        }
#endif
        /// <summary>The links property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public OpenApiKiotaClientExample.GeneratedCode.Models.ResourceTopLevelLinks? Links
        {
            get { return BackingStore?.Get<OpenApiKiotaClientExample.GeneratedCode.Models.ResourceTopLevelLinks?>("links"); }
            set { BackingStore?.Set("links", value); }
        }
#nullable restore
#else
        public OpenApiKiotaClientExample.GeneratedCode.Models.ResourceTopLevelLinks Links
        {
            get { return BackingStore?.Get<OpenApiKiotaClientExample.GeneratedCode.Models.ResourceTopLevelLinks>("links"); }
            set { BackingStore?.Set("links", value); }
        }
#endif
        /// <summary>The meta property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public OpenApiKiotaClientExample.GeneratedCode.Models.Meta? Meta
        {
            get { return BackingStore?.Get<OpenApiKiotaClientExample.GeneratedCode.Models.Meta?>("meta"); }
            set { BackingStore?.Set("meta", value); }
        }
#nullable restore
#else
        public OpenApiKiotaClientExample.GeneratedCode.Models.Meta Meta
        {
            get { return BackingStore?.Get<OpenApiKiotaClientExample.GeneratedCode.Models.Meta>("meta"); }
            set { BackingStore?.Set("meta", value); }
        }
#endif
        /// <summary>
        /// Instantiates a new <see cref="OpenApiKiotaClientExample.GeneratedCode.Models.NullablePersonSecondaryResponseDocument"/> and sets the default values.
        /// </summary>
        public NullablePersonSecondaryResponseDocument()
        {
            BackingStore = BackingStoreFactorySingleton.Instance.CreateBackingStore();
        }
        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// </summary>
        /// <returns>A <see cref="OpenApiKiotaClientExample.GeneratedCode.Models.NullablePersonSecondaryResponseDocument"/></returns>
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        public static OpenApiKiotaClientExample.GeneratedCode.Models.NullablePersonSecondaryResponseDocument CreateFromDiscriminatorValue(IParseNode parseNode)
        {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            return new OpenApiKiotaClientExample.GeneratedCode.Models.NullablePersonSecondaryResponseDocument();
        }
        /// <summary>
        /// The deserialization information for the current model
        /// </summary>
        /// <returns>A IDictionary&lt;string, Action&lt;IParseNode&gt;&gt;</returns>
        public virtual IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
        {
            return new Dictionary<string, Action<IParseNode>>
            {
                { "data", n => { Data = n.GetObjectValue<OpenApiKiotaClientExample.GeneratedCode.Models.PersonDataInResponse>(OpenApiKiotaClientExample.GeneratedCode.Models.PersonDataInResponse.CreateFromDiscriminatorValue); } },
                { "included", n => { Included = n.GetCollectionOfObjectValues<OpenApiKiotaClientExample.GeneratedCode.Models.DataInResponse>(OpenApiKiotaClientExample.GeneratedCode.Models.DataInResponse.CreateFromDiscriminatorValue)?.ToList(); } },
                { "links", n => { Links = n.GetObjectValue<OpenApiKiotaClientExample.GeneratedCode.Models.ResourceTopLevelLinks>(OpenApiKiotaClientExample.GeneratedCode.Models.ResourceTopLevelLinks.CreateFromDiscriminatorValue); } },
                { "meta", n => { Meta = n.GetObjectValue<OpenApiKiotaClientExample.GeneratedCode.Models.Meta>(OpenApiKiotaClientExample.GeneratedCode.Models.Meta.CreateFromDiscriminatorValue); } },
            };
        }
        /// <summary>
        /// Serializes information the current object
        /// </summary>
        /// <param name="writer">Serialization writer to use to serialize this model</param>
        public virtual void Serialize(ISerializationWriter writer)
        {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            writer.WriteObjectValue<OpenApiKiotaClientExample.GeneratedCode.Models.PersonDataInResponse>("data", Data);
            writer.WriteCollectionOfObjectValues<OpenApiKiotaClientExample.GeneratedCode.Models.DataInResponse>("included", Included);
            writer.WriteObjectValue<OpenApiKiotaClientExample.GeneratedCode.Models.ResourceTopLevelLinks>("links", Links);
            writer.WriteObjectValue<OpenApiKiotaClientExample.GeneratedCode.Models.Meta>("meta", Meta);
        }
    }
}