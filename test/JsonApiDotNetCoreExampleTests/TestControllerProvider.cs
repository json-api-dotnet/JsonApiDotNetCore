using System;
using System.Reflection;
using JsonApiDotNetCore;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace JsonApiDotNetCoreExampleTests
{
    internal sealed class TestControllerProvider : ControllerFeatureProvider
    {
        private readonly string _controllerNamespace;

        public TestControllerProvider(string controllerNamespace)
        {
            ArgumentGuard.NotNull(controllerNamespace, nameof(controllerNamespace));

            _controllerNamespace = controllerNamespace;
        }

        protected override bool IsController(TypeInfo typeInfo)
        {
            bool isController = base.IsController(typeInfo);

            bool controllerInAllowedNamespace = isController && typeInfo.Namespace!.StartsWith(_controllerNamespace, StringComparison.Ordinal);
            return controllerInAllowedNamespace;
        }
    }
}
