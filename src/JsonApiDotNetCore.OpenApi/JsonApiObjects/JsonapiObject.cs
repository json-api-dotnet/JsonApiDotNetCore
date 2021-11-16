using System.Collections.Generic;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.OpenApi.JsonApiObjects
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    internal sealed class JsonapiObject
    {
        public string Version { get; set; } = null!;

        public ICollection<string> Ext { get; set; } = null!;

        public ICollection<string> Profile { get; set; } = null!;

        public IDictionary<string, object> Meta { get; set; } = null!;
    }
}
