using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Controllers;

[PublicAPI]
[AttributeUsage(AttributeTargets.Parameter)]
public sealed class PreserveEmptyStringAttribute : DisplayFormatAttribute
{
    public PreserveEmptyStringAttribute()
    {
        // Workaround for https://github.com/dotnet/aspnetcore/issues/29948#issuecomment-1898216682
        ConvertEmptyStringToNull = false;
    }
}
