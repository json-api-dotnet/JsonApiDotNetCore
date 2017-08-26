using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Models.Pointers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Extensions;

namespace JsonApiDotNetCore.Services.Operations
{
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
            _data.Id = GetPointerValue(_data.Id, parentDoc);
            _data.Type = GetPointerValue(_data.Type, parentDoc);

            if (_data.Relationships != null)
            {
                foreach (var relationshipDictionary in _data.Relationships)
                {
                    if (relationshipDictionary.Value.IsHasMany)
                    {
                        foreach (var relationship in relationshipDictionary.Value.ManyData)
                            ReplaceDictionaryPointers(relationship, parentDoc);
                    }
                    else
                    {
                        ReplaceDictionaryPointers(relationshipDictionary.Value.SingleData, parentDoc);
                    }
                }
            }
        }

        private void ReplaceDictionaryPointers(Dictionary<string, object> relationship, List<TPointerBase> parentDoc)
        {
            if (relationship.ContainsKey("id"))
                relationship["id"] = GetPointerValue(relationship["id"], parentDoc);

            if (relationship.ContainsKey("type"))
                relationship["type"] = GetPointerValue(relationship["type"], parentDoc);
        }

        private object GetPointerValue(object reference, List<TPointerBase> parentDoc)
        {
            if (reference is JObject jObj)
                if (jObj.TryParse<TPointer, TPointerBase>(Pointer<TPointerBase>.JsonSchema, out Pointer<TPointerBase> pointer))
                    return pointer.GetValue(parentDoc);

            return reference;
        }
    }
}