using System;
using System.Reflection;
using System.Threading;

namespace JsonApiDotNetCore.OpenApi
{
    internal static class ParameterInfoExtensions
    {
        private static readonly Lazy<FieldInfo> NameField = new(() =>
            typeof(ParameterInfo).GetField("NameImpl", BindingFlags.Instance | BindingFlags.NonPublic), LazyThreadSafetyMode.ExecutionAndPublication);

        private static readonly Lazy<FieldInfo> ParameterTypeField = new(() =>
            typeof(ParameterInfo).GetField("ClassImpl", BindingFlags.Instance | BindingFlags.NonPublic), LazyThreadSafetyMode.ExecutionAndPublication);

        public static ParameterInfo WithName(this ParameterInfo source, string name)
        {
            ArgumentGuard.NotNull(source, nameof(source));
            ArgumentGuard.NotNullNorEmpty(name, nameof(name));

            var cloned = (ParameterInfo)source.MemberwiseClone();
            NameField.Value.SetValue(cloned, name);

            return cloned;
        }

        public static ParameterInfo WithParameterType(this ParameterInfo source, Type parameterType)
        {
            ArgumentGuard.NotNull(source, nameof(source));
            ArgumentGuard.NotNull(parameterType, nameof(parameterType));

            var cloned = (ParameterInfo)source.MemberwiseClone();
            ParameterTypeField.Value.SetValue(cloned, parameterType);

            return cloned;
        }
    }
}
