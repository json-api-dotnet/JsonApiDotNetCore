using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ApiRequestFormatMedataProvider
{
    internal sealed class ApiExplorerConvention : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            controller.ApiExplorer.IsVisible = true;
        }
    }
}
