using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExample.Models;

[Resource]
public sealed class MyDto : Identifiable<int>
{
    [Attr]
    public int? EmployeeId { get; set; }

    [Attr]
    public int SomeInt { get; set; }

    [Attr]
    public string? SomeString { get; set; }
}
