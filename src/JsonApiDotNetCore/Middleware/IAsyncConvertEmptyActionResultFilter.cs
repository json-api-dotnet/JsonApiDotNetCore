using Microsoft.AspNetCore.Mvc.Filters;

namespace JsonApiDotNetCore.Middleware
{
    /// <summary>
    /// Converts action result without parameters into action result with null parameter.
    /// For example: return NotFound() -> return NotFound(null)
    /// This ensures our formatter is invoked, where we'll build a JSON:API compliant response.
    /// For details, see: https://github.com/dotnet/aspnetcore/issues/16969
    /// </summary>
    public interface IAsyncConvertEmptyActionResultFilter : IAsyncAlwaysRunResultFilter { }
}
