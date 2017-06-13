using System.Reflection;
using JsonApiDotNetCore.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace JsonApiDotNetCore.Serialization
{
    public class DasherizedResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            property.PropertyName = property.PropertyName.Dasherize();

            return property;
        }
    }
}
