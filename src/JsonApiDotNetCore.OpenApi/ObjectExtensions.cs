using System;
using System.Reflection;
using System.Threading;

namespace JsonApiDotNetCore.OpenApi
{
    internal static class ObjectExtensions
    {
        private static readonly Lazy<MethodInfo> MemberwiseCloneMethod =
            new(() => typeof(object).GetMethod(nameof(MemberwiseClone), BindingFlags.Instance | BindingFlags.NonPublic)!,
                LazyThreadSafetyMode.ExecutionAndPublication);

        public static object MemberwiseClone(this object source)
        {
            ArgumentGuard.NotNull(source, nameof(source));

            return MemberwiseCloneMethod.Value.Invoke(source, null)!;
        }
    }
}
