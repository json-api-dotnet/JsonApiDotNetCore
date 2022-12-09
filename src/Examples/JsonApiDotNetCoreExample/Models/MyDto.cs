using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExample.Models;

[Resource]
public sealed class MyDto : Identifiable<int>
{
    [Attr]
    public string? EmployeeId { get; set; }
}
