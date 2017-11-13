using System;
using System.Reflection;
using JsonApiDotNetCore.Internal;

namespace JsonApiDotNetCore.Services
{
    public interface IControllerContext
    {
        Type ControllerType { get; set; }
        ContextEntity RequestEntity { get; set; }
        TAttribute GetControllerAttribute<TAttribute>() where TAttribute : Attribute;
    }

    public class ControllerContext : IControllerContext
    {
        public Type ControllerType { get; set; }
        public ContextEntity RequestEntity { get; set; }

        public TAttribute GetControllerAttribute<TAttribute>() where TAttribute : Attribute
        {
            var attribute = ControllerType.GetTypeInfo().GetCustomAttribute(typeof(TAttribute));
            return attribute == null ? null : (TAttribute)attribute;
        }
    }
}