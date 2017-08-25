using System.Collections.Generic;
using JsonApiDotNetCore.Models.Pointers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace JsonApiDotNetCore.Models
{
    public class DocumentData
    {
        [JsonProperty("type")]
        public object Type { get; set; }

        [JsonProperty("id")]
        public object Id { get; set; }

        [JsonProperty("attributes")]
        public Dictionary<string, object> Attributes { get; set; }

        [JsonProperty("relationships")]
        public Dictionary<string, RelationshipData> Relationships { get; set; }
    }

    public class DocumentDataPointerReplacement<TPointer, TPointerBase>
        where TPointer : Pointer<TPointerBase>, new()
    {
        private readonly DocumentData _data;

        public DocumentDataPointerReplacement(DocumentData data)
        {
            _data = data;
        }

        public void ReplacePointers(List<TPointerBase> parentDoc)
        {
            ReplacePointer(_data.Id, parentDoc);
            ReplacePointer(_data.Type, parentDoc);
        }

        private void ReplacePointer(object reference, List<TPointerBase> parentDoc)
        {
            if (reference is JObject jObj)
                if (jObj.TryParse<TPointer, TPointerBase>(Pointer<TPointerBase>.JsonSchema, out Pointer<TPointerBase> pointer))
                    reference = pointer.GetValue(parentDoc);
        }
    }
}

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