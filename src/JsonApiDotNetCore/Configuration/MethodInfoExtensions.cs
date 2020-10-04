using System;
using System.Reflection;
using System.Threading.Tasks;

namespace JsonApiDotNetCore.Configuration
{
    public static class MethodInfoExtensions
    {
        public static async Task<object> InvokeAsync(this MethodInfo methodInfo, object obj, params object[] parameters)
        {
            if (methodInfo == null) throw new ArgumentNullException(nameof(methodInfo));

            var task = (Task)methodInfo.Invoke(obj, parameters);
            await task.ConfigureAwait(false);
            var resultProperty = task.GetType().GetProperty("Result");
            return resultProperty.GetValue(task);
        }
    }
}
