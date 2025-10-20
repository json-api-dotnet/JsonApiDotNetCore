using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JsonApiDotNetCore.Middleware;

/// <summary>
/// Converts action result without parameters into action result with null parameter.
/// </summary>
/// <remarks>
/// This basically turns calls such as
/// <c>
/// return NotFound()
/// </c>
/// into
/// <c>
/// return NotFound(null)
/// </c>
/// , so that our formatter is invoked, where we'll build a JSON:API compliant response. For details, see:
/// https://github.com/dotnet/aspnetcore/issues/16969
/// </remarks>
[PublicAPI]
public interface IAsyncConvertEmptyActionResultFilter : IAsyncAlwaysRunResultFilter;
