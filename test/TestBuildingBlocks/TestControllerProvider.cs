using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace TestBuildingBlocks;

internal sealed class TestControllerProvider : ControllerFeatureProvider
{
    private readonly HashSet<Type> _allowedControllerTypes = [];

    internal HashSet<Assembly> ControllerAssemblies { get; } = [];

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
