using Newtonsoft.Json;
using Newtonsoft.Json.Schema;

namespace JsonApiDotNetCore.Models.Pointers
{
    internal abstract class Pointer<TPointerBase>
    {
        public static JSchema JsonSchema { get; } = JSchema.Parse("{ 'pointer': {'type': 'string'} }");

        /// <summary>
        /// Location represented by the pointer
        /// </summary>
        /// <example>/operations/0/data/id</example>
        [JsonProperty("pointer")]
        public string PointerAddress { get; set; }

        /// <summary>
        /// Get the value located at the PointerAddress in the supplied object
        /// </summary>
        public abstract object GetValue(object root);
    }
}
