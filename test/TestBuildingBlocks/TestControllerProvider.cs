using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace TestBuildingBlocks
{
    public sealed class TestControllerProvider : ControllerFeatureProvider
    {
        internal readonly ISet<Assembly> ControllerAssemblies = new HashSet<Assembly>();
        private readonly IList<Type> _namespaceEntryPoints  = new List<Type>();
        private readonly IList<Type> _allowedControllerTypes = new List<Type>();
        private string[] _namespaces;

        public void AddController<TController>()
        {
            Type controller = typeof(TController);
            _allowedControllerTypes.Add(controller);
            ControllerAssemblies.Add(controller.Assembly);
        }

        public void AddNamespaceEntrypoint(Type entrypoint)
        {
            _namespaceEntryPoints.Add(entrypoint);
            ControllerAssemblies.Add(entrypoint.Assembly);
        }

        protected override bool IsController(TypeInfo typeInfo)
        {
            if (!base.IsController(typeInfo))
            {
                return false;
            }

            _namespaces ??= _namespaceEntryPoints.Select(type => type.Namespace).ToArray();

            return _allowedControllerTypes.Contains(typeInfo) || _namespaces.Any(@namespace => typeInfo.Namespace!.StartsWith(@namespace, StringComparison.Ordinal));
        }
    }
}
