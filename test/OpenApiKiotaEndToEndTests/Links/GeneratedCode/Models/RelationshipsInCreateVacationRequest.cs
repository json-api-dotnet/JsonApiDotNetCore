// <auto-generated/>
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions.Store;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
namespace OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models
{
    #pragma warning disable CS1591
    public class RelationshipsInCreateVacationRequest : IBackedModel, IParsable
    #pragma warning restore CS1591
    {
        /// <summary>The accommodation property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.ToOneAccommodationInRequest? Accommodation
        {
            get { return BackingStore?.Get<OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.ToOneAccommodationInRequest?>("accommodation"); }
            set { BackingStore?.Set("accommodation", value); }
        }
#nullable restore
#else
        public OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.ToOneAccommodationInRequest Accommodation
        {
            get { return BackingStore?.Get<OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.ToOneAccommodationInRequest>("accommodation"); }
            set { BackingStore?.Set("accommodation", value); }
        }
#endif
        /// <summary>Stores model information.</summary>
        public IBackingStore BackingStore { get; private set; }
        /// <summary>The excursions property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.ToManyExcursionInRequest? Excursions
        {
            get { return BackingStore?.Get<OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.ToManyExcursionInRequest?>("excursions"); }
            set { BackingStore?.Set("excursions", value); }
        }
#nullable restore
#else
        public OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.ToManyExcursionInRequest Excursions
        {
            get { return BackingStore?.Get<OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.ToManyExcursionInRequest>("excursions"); }
            set { BackingStore?.Set("excursions", value); }
        }
#endif
        /// <summary>The transport property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.NullableToOneTransportInRequest? Transport
        {
            get { return BackingStore?.Get<OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.NullableToOneTransportInRequest?>("transport"); }
            set { BackingStore?.Set("transport", value); }
        }
#nullable restore
#else
        public OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.NullableToOneTransportInRequest Transport
        {
            get { return BackingStore?.Get<OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.NullableToOneTransportInRequest>("transport"); }
            set { BackingStore?.Set("transport", value); }
        }
#endif
        /// <summary>
        /// Instantiates a new <see cref="OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.RelationshipsInCreateVacationRequest"/> and sets the default values.
        /// </summary>
        public RelationshipsInCreateVacationRequest()
        {
            BackingStore = BackingStoreFactorySingleton.Instance.CreateBackingStore();
        }
        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// </summary>
        /// <returns>A <see cref="OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.RelationshipsInCreateVacationRequest"/></returns>
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        public static OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.RelationshipsInCreateVacationRequest CreateFromDiscriminatorValue(IParseNode parseNode)
        {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            return new OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.RelationshipsInCreateVacationRequest();
        }
        /// <summary>
        /// The deserialization information for the current model
        /// </summary>
        /// <returns>A IDictionary&lt;string, Action&lt;IParseNode&gt;&gt;</returns>
        public virtual IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
        {
            return new Dictionary<string, Action<IParseNode>>
            {
                { "accommodation", n => { Accommodation = n.GetObjectValue<OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.ToOneAccommodationInRequest>(OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.ToOneAccommodationInRequest.CreateFromDiscriminatorValue); } },
                { "excursions", n => { Excursions = n.GetObjectValue<OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.ToManyExcursionInRequest>(OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.ToManyExcursionInRequest.CreateFromDiscriminatorValue); } },
                { "transport", n => { Transport = n.GetObjectValue<OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.NullableToOneTransportInRequest>(OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.NullableToOneTransportInRequest.CreateFromDiscriminatorValue); } },
            };
        }
        /// <summary>
        /// Serializes information the current object
        /// </summary>
        /// <param name="writer">Serialization writer to use to serialize this model</param>
        public virtual void Serialize(ISerializationWriter writer)
        {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            writer.WriteObjectValue<OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.ToOneAccommodationInRequest>("accommodation", Accommodation);
            writer.WriteObjectValue<OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.ToManyExcursionInRequest>("excursions", Excursions);
            writer.WriteObjectValue<OpenApiKiotaEndToEndTests.Links.GeneratedCode.Models.NullableToOneTransportInRequest>("transport", Transport);
        }
    }
}