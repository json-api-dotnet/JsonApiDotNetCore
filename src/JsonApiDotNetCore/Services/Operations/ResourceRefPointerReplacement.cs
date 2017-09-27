using System.Collections.Generic;
using JsonApiDotNetCore.Models.Pointers;
using Newtonsoft.Json.Linq;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Models.Operations;

namespace JsonApiDotNetCore.Services.Operations
{
    public class ResourceRefPointerReplacement<TPointer, TPointerBase>
        where TPointer : Pointer<TPointerBase>, new()
    {
        private readonly ResourceReference _ref;

        public ResourceRefPointerReplacement(ResourceReference data)
        {
            _ref = data;
        }

        public void ReplacePointers(List<TPointerBase> parentDoc)
        {
            _ref.Id = GetPointerValue(_ref.Id, parentDoc);
            _ref.Type = GetPointerValue(_ref.Type, parentDoc);
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