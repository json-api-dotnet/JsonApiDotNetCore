using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace JsonApiDotNetCoreExampleClient
{
    internal static class Program
    {
        private const string BaseUrl = "http://localhost:14140";

        private static async Task Main()
        {
            using var httpClient = new HttpClient();

            ExampleApiClient exampleApiClient = new(BaseUrl, httpClient);

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
