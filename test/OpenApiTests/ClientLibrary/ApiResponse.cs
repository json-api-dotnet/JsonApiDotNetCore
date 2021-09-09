using System;
using System.Threading.Tasks;
using JsonApiDotNetCore;
using OpenApiTests.ClientLibrary.GeneratedCode;

#pragma warning disable AV1008 // Class should not be static

namespace OpenApiTests.ClientLibrary
{
    internal static class ApiResponse
    {
        public static async Task<TResponse> TranslateAsync<TResponse>(Func<Task<TResponse>> operation)
        {
            // Workaround for https://github.com/RicoSuter/NSwag/issues/2499

            ArgumentGuard.NotNull(operation, nameof(operation));

            try
            {
                return await operation();
            }
            catch (ApiException exception)
            {
                if (exception.StatusCode != 204)
                {
                    throw;
                }

                return default;
            }
        }
    }
}
