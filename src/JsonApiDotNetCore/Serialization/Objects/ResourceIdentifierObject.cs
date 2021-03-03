using System.Text;
using Newtonsoft.Json;

namespace JsonApiDotNetCore.Serialization.Objects
{
    public class ResourceIdentifierObject
    {
        [JsonProperty("type", Order = -4)]
        public string Type { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore, Order = -3)]
        public string Id { get; set; }

        [JsonProperty("lid", NullValueHandling = NullValueHandling.Ignore, Order = -2)]
        public string Lid { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();

            WriteMembers(builder);
            builder.Insert(0, GetType().Name + ": ");

            return builder.ToString();
        }

        protected virtual void WriteMembers(StringBuilder builder)
        {
            WriteMember(builder, "type", Type);
            WriteMember(builder, "id", Id);
            WriteMember(builder, "lid", Lid);
        }

        protected static void WriteMember(StringBuilder builder, string memberName, string memberValue)
        {
            if (memberValue != null)
            {
                if (builder.Length > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(memberName);
                builder.Append("=\"");
                builder.Append(memberValue);
                builder.Append('"');
            }
        }
    }
}
