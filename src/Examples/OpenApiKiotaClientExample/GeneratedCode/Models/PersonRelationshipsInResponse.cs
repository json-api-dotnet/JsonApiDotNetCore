// <auto-generated/>
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions.Store;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
namespace OpenApiKiotaClientExample.GeneratedCode.Models {
    #pragma warning disable CS1591
    public class PersonRelationshipsInResponse : IBackedModel, IParsable 
    #pragma warning restore CS1591
    {
        /// <summary>The assignedTodoItems property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public ToManyTodoItemInResponse? AssignedTodoItems {
            get { return BackingStore?.Get<ToManyTodoItemInResponse?>("assignedTodoItems"); }
            set { BackingStore?.Set("assignedTodoItems", value); }
        }
#nullable restore
#else
        public ToManyTodoItemInResponse AssignedTodoItems {
            get { return BackingStore?.Get<ToManyTodoItemInResponse>("assignedTodoItems"); }
            set { BackingStore?.Set("assignedTodoItems", value); }
        }
#endif
        /// <summary>Stores model information.</summary>
        public IBackingStore BackingStore { get; private set; }
        /// <summary>The ownedTodoItems property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public ToManyTodoItemInResponse? OwnedTodoItems {
            get { return BackingStore?.Get<ToManyTodoItemInResponse?>("ownedTodoItems"); }
            set { BackingStore?.Set("ownedTodoItems", value); }
        }
#nullable restore
#else
        public ToManyTodoItemInResponse OwnedTodoItems {
            get { return BackingStore?.Get<ToManyTodoItemInResponse>("ownedTodoItems"); }
            set { BackingStore?.Set("ownedTodoItems", value); }
        }
#endif
        /// <summary>
        /// Instantiates a new <see cref="PersonRelationshipsInResponse"/> and sets the default values.
        /// </summary>
        public PersonRelationshipsInResponse()
        {
            BackingStore = BackingStoreFactorySingleton.Instance.CreateBackingStore();
        }
        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// </summary>
        /// <returns>A <see cref="PersonRelationshipsInResponse"/></returns>
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        public static PersonRelationshipsInResponse CreateFromDiscriminatorValue(IParseNode parseNode)
        {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            return new PersonRelationshipsInResponse();
        }
        /// <summary>
        /// The deserialization information for the current model
        /// </summary>
        /// <returns>A IDictionary&lt;string, Action&lt;IParseNode&gt;&gt;</returns>
        public virtual IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
        {
            return new Dictionary<string, Action<IParseNode>>
            {
                {"assignedTodoItems", n => { AssignedTodoItems = n.GetObjectValue<ToManyTodoItemInResponse>(ToManyTodoItemInResponse.CreateFromDiscriminatorValue); } },
                {"ownedTodoItems", n => { OwnedTodoItems = n.GetObjectValue<ToManyTodoItemInResponse>(ToManyTodoItemInResponse.CreateFromDiscriminatorValue); } },
            };
        }
        /// <summary>
        /// Serializes information the current object
        /// </summary>
        /// <param name="writer">Serialization writer to use to serialize this model</param>
        public virtual void Serialize(ISerializationWriter writer)
        {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            writer.WriteObjectValue<ToManyTodoItemInResponse>("assignedTodoItems", AssignedTodoItems);
            writer.WriteObjectValue<ToManyTodoItemInResponse>("ownedTodoItems", OwnedTodoItems);
        }
    }
}