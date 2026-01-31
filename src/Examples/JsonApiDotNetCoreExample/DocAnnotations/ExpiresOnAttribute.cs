namespace JsonApiDotNetCoreExample.DocAnnotations;

[AttributeUsage(AttributeTargets.Method)]
internal sealed class ExpiresOnAttribute(string dateValue) : Attribute
{
    public DateOnly Value { get; } = DateOnly.Parse(dateValue);
}
