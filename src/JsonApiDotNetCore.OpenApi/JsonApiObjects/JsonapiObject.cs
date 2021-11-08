using System.Collections.Generic;
using JetBrains.Annotations;

#pragma warning disable 8618 // Non-nullable member is uninitialized.

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    internal sealed class JsonapiObject
    {
        public string Version { get; set; }

        public ICollection<string> Ext { get; set; }

        public ICollection<string> Profile { get; set; }

        public IDictionary<string, object> Meta { get; set; }
    }
}
