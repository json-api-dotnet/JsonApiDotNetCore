using System;
using System.Net.Http;
using System.Threading.Tasks;
using JsonApiDotNetCoreExampleClient.GeneratedCode;

#pragma warning disable AV1210 // Catch a specific exception instead of Exception, SystemException or ApplicationException

namespace JsonApiDotNetCoreExampleClient
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class Program
    {
        private static async Task Main()
        {
            using var httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:14140")
            };

            ExampleApiClient exampleApiClient = new(httpClient);

            try
            {
                const int nonExistingId = int.MaxValue;
                await exampleApiClient.Delete_personAsync(nonExistingId);
            }
            catch (ApiException exception)
            {
                Console.WriteLine(exception.Response);
            }

            Console.WriteLine("Press any key to close.");
            Console.ReadKey();
        }
    }
}
