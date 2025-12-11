using System.Text;

// Justification: The CA1852 analyzer doesn't take auto-generated code into account, presumably for improved performance.
#pragma warning disable CA1852 // Type can be sealed because it has no subtypes in its containing assembly and is not externally visible

namespace OpenApiNSwagEndToEndTests.MixedControllers.GeneratedCode;

internal partial class MixedControllersClient
{
    // ReSharper disable once UnusedParameterInPartialMethod
    partial void PrepareRequest(HttpClient client, HttpRequestMessage request, StringBuilder urlBuilder)
    {
        if (request.Method == HttpMethod.Patch && urlBuilder.ToString().EndsWith("/cupOfCoffees/batch", StringComparison.Ordinal))
        {
            // Workaround: NSwag assumes a PATCH request must always send a request body, despite our OpenAPI document not specifying one.
            request.Content = null;
        }
    }
}
