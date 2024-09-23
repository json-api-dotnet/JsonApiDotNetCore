// <auto-generated/>
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions.Store;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
namespace OpenApiKiotaEndToEndTests.ModelStateValidation.GeneratedCode.Models
{
    #pragma warning disable CS1591
    public class DataInCreateSocialMediaAccountRequest : IBackedModel, IParsable
    #pragma warning restore CS1591
    {
        /// <summary>The attributes property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public OpenApiKiotaEndToEndTests.ModelStateValidation.GeneratedCode.Models.AttributesInCreateSocialMediaAccountRequest? Attributes
        {
            get { return BackingStore?.Get<OpenApiKiotaEndToEndTests.ModelStateValidation.GeneratedCode.Models.AttributesInCreateSocialMediaAccountRequest?>("attributes"); }
            set { BackingStore?.Set("attributes", value); }
        }
#nullable restore
#else
        public OpenApiKiotaEndToEndTests.ModelStateValidation.GeneratedCode.Models.AttributesInCreateSocialMediaAccountRequest Attributes
        {
            get { return BackingStore?.Get<OpenApiKiotaEndToEndTests.ModelStateValidation.GeneratedCode.Models.AttributesInCreateSocialMediaAccountRequest>("attributes"); }
            set { BackingStore?.Set("attributes", value); }
        }
#endif
        /// <summary>Stores model information.</summary>
        public IBackingStore BackingStore { get; private set; }
        /// <summary>The type property</summary>
        public OpenApiKiotaEndToEndTests.ModelStateValidation.GeneratedCode.Models.SocialMediaAccountResourceType? Type
        {
            get { return BackingStore?.Get<OpenApiKiotaEndToEndTests.ModelStateValidation.GeneratedCode.Models.SocialMediaAccountResourceType?>("type"); }
            set { BackingStore?.Set("type", value); }
        }
        /// <summary>
        /// Instantiates a new <see cref="OpenApiKiotaEndToEndTests.ModelStateValidation.GeneratedCode.Models.DataInCreateSocialMediaAccountRequest"/> and sets the default values.
        /// </summary>
        public DataInCreateSocialMediaAccountRequest()
        {
            BackingStore = BackingStoreFactorySingleton.Instance.CreateBackingStore();
        }
        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// </summary>
        /// <returns>A <see cref="OpenApiKiotaEndToEndTests.ModelStateValidation.GeneratedCode.Models.DataInCreateSocialMediaAccountRequest"/></returns>
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        public static OpenApiKiotaEndToEndTests.ModelStateValidation.GeneratedCode.Models.DataInCreateSocialMediaAccountRequest CreateFromDiscriminatorValue(IParseNode parseNode)
        {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            return new OpenApiKiotaEndToEndTests.ModelStateValidation.GeneratedCode.Models.DataInCreateSocialMediaAccountRequest();
        }
        /// <summary>
        /// The deserialization information for the current model
        /// </summary>
        /// <returns>A IDictionary&lt;string, Action&lt;IParseNode&gt;&gt;</returns>
        public virtual IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
        {
            return new Dictionary<string, Action<IParseNode>>
            {
                { "attributes", n => { Attributes = n.GetObjectValue<OpenApiKiotaEndToEndTests.ModelStateValidation.GeneratedCode.Models.AttributesInCreateSocialMediaAccountRequest>(OpenApiKiotaEndToEndTests.ModelStateValidation.GeneratedCode.Models.AttributesInCreateSocialMediaAccountRequest.CreateFromDiscriminatorValue); } },
                { "type", n => { Type = n.GetEnumValue<OpenApiKiotaEndToEndTests.ModelStateValidation.GeneratedCode.Models.SocialMediaAccountResourceType>(); } },
            };
        }
        /// <summary>
        /// Serializes information the current object
        /// </summary>
        /// <param name="writer">Serialization writer to use to serialize this model</param>
        public virtual void Serialize(ISerializationWriter writer)
        {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            writer.WriteObjectValue<OpenApiKiotaEndToEndTests.ModelStateValidation.GeneratedCode.Models.AttributesInCreateSocialMediaAccountRequest>("attributes", Attributes);
            writer.WriteEnumValue<OpenApiKiotaEndToEndTests.ModelStateValidation.GeneratedCode.Models.SocialMediaAccountResourceType>("type", Type);
        }
    }
}