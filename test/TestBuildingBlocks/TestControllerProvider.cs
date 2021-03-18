using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace TestBuildingBlocks
{
    internal sealed class TestControllerProvider : ControllerFeatureProvider
    {
        private readonly ISet<Type> _allowedControllerTypes = new HashSet<Type>();
        internal ISet<Assembly> ControllerAssemblies { get; } = new HashSet<Assembly>();

        public void AddController(Type controller)
        {
            _allowedControllerTypes.Add(controller);
            ControllerAssemblies.Add(controller.Assembly);
        }

        protected override bool IsController(TypeInfo typeInfo)
        {
            return _allowedControllerTypes.Contains(typeInfo);
        }
    }
}
