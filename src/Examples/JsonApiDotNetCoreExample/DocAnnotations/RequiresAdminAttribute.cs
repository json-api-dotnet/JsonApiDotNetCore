namespace JsonApiDotNetCoreExample.DocAnnotations;

[AttributeUsage(AttributeTargets.Method)]
internal sealed class RequiresAdminAttribute : Attribute;
