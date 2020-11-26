using System.Text;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization.Objects
{
    public sealed class AtomicReference : ResourceIdentifierObject
    {
        [JsonProperty("relationship", NullValueHandling = NullValueHandling.Ignore)]
        public string Relationship { get; set; }

        protected override void WriteMembers(StringBuilder builder)
        {
            base.WriteMembers(builder);
            WriteMember(builder, "relationship", Relationship);
        }
    }
}
