using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace TestBuildingBlocks
{
    public sealed class TestControllerProvider : ControllerFeatureProvider
    {
        public IList<Type> NamespaceEntryPoints { get; } = new List<Type>();
        public IList<Type> AllowedControllerTypes { get; } = new List<Type>();

        private string[] _namespaces;

        public void AddController<TControllerType>()
        {
            AllowedControllerTypes.Add(typeof(TControllerType));
        }

        protected override bool IsController(TypeInfo typeInfo)
        {
            if (!base.IsController(typeInfo))
            {
                return false;
            }

            _namespaces ??= NamespaceEntryPoints.Select(type => type.Namespace).ToArray();

            return AllowedControllerTypes.Contains(typeInfo) || _namespaces.Any(@namespace => typeInfo.Namespace!.StartsWith(@namespace, StringComparison.Ordinal));
        }
    }
}
