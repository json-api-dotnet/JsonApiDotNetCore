using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using JsonApiDotNetCore.Models.Pointers;

namespace JsonApiDotNetCore.Extensions
{
    public static class JObjectExtensions
    {
        public static bool TryParse<TPointer, TPointerBase>(this JObject obj, JSchema schema, out Pointer<TPointerBase> pointer)
        where TPointer : Pointer<TPointerBase>, new()
        {
            if (obj.IsValid(schema))
            {
                pointer = obj.ToObject<TPointer>();
                return true;
            }

            pointer = null;
            return false;
        }
    }
}