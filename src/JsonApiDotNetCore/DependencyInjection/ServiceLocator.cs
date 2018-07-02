using System;
using System.Threading;

namespace JsonApiDotNetCore.DependencyInjection
{
    internal class ServiceLocator
    {
        public static AsyncLocal<IServiceProvider> _scopedProvider = new AsyncLocal<IServiceProvider>();
        public static void Initialize(IServiceProvider serviceProvider) => _scopedProvider.Value = serviceProvider;
        
        public static object GetService(Type type)
            => _scopedProvider.Value != null
                ? _scopedProvider.Value.GetService(type)
                : throw new InvalidOperationException(
                    $"Service locator has not been initialized for the current asynchronous flow. Call {nameof(Initialize)} first."
                );
    }
}
