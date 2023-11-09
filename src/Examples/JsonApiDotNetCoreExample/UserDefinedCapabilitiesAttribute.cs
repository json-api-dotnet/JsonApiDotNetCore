using JetBrains.Annotations;

namespace JsonApiDotNetCoreExample;

[UsedImplicitly]
[AttributeUsage(AttributeTargets.Property)]
public sealed class UserDefinedCapabilitiesAttribute : Attribute
{
    public bool AllowCreateRelationship { get; set; }
    public bool AllowUpdateRelationship { get; set; }
}
