using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

// ReSharper disable CheckNamespace
#pragma warning disable AV1505 // Namespace should match with assembly name

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource]
public sealed class Global : Identifiable<int>
{
    [Attr]
    public string? Value { get; set; }
}
