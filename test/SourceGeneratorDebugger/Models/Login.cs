using System;
using JetBrains.Annotations;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

#pragma warning disable AV1505 // Namespace should match with assembly name

namespace SourceGeneratorDebugger.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    [Resource(GenerateControllerEndpoints = JsonApiEndpoints.Command)]
    public sealed class Login : Identifiable<int>
    {
        [Attr]
        public DateTimeOffset Time { get; set; }
    }
}

namespace Some.Other.Path
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    [Resource(GenerateControllerEndpoints = JsonApiEndpoints.Command)]
    public sealed class Login : Identifiable<int>
    {
        [Attr]
        public DateTimeOffset Time { get; set; }
    }
}
