using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCoreExample;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace JsonApiDotNetCoreExampleTests
{
    /// <summary>
    /// A test context that creates a new database and server instance before running tests and cleans up afterwards.
    /// You can either use this as a fixture on your tests class (init/cleanup runs once before/after all tests) or
    /// have your tests class inherit from it (init/cleanup runs once before/after each test). See
    /// <see href="https://xunit.net/docs/shared-context"/> for details on shared context usage.
    /// </summary>
    /// <typeparam name="TStartup">The server Startup class, which can be defined in the test project.</typeparam>
    /// <typeparam name="TDbContext">The EF Core database context, which can be defined in the test project.</typeparam>
    public class IntegrationTestContext<TStartup, TDbContext> : IDisposable
        where TStartup : class
        where TDbContext : DbContext
    {
        private readonly Lazy<WebApplicationFactory<EmptyStartup>> _lazyFactory;
        private Action<ILoggingBuilder> _loggingConfiguration;
        private Action<IServiceCollection> _beforeServicesConfiguration;
        private Action<IServiceCollection> _afterServicesConfiguration;

        public WebApplicationFactory<EmptyStartup> Factory => _lazyFactory.Value;

        public IntegrationTestContext()
        {
            _lazyFactory = new Lazy<WebApplicationFactory<EmptyStartup>>(CreateFactory);
        }

        private WebApplicationFactory<EmptyStartup> CreateFactory()
        {
            string postgresPassword = Environment.GetEnvironmentVariable("PGPASSWORD") ?? "postgres";
            string dbConnectionString =
                $"Host=localhost;Port=5432;Database=JsonApiTest-{Guid.NewGuid():N};User ID=postgres;Password={postgresPassword}";

            var factory = new IntegrationTestWebApplicationFactory();

            factory.ConfigureLogging(_loggingConfiguration);

            factory.ConfigureServicesBeforeStartup(services =>
            {
                _beforeServicesConfiguration?.Invoke(services);

                services.AddDbContext<TDbContext>(options =>
                {
                    options.UseNpgsql(dbConnectionString,
                        postgresOptions => postgresOptions.SetPostgresVersion(new Version(9, 6)));

                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                });
            });

            factory.ConfigureServicesAfterStartup(_afterServicesConfiguration);

            using IServiceScope scope = factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
            dbContext.Database.EnsureCreated();

            return factory;
        }

        public void Dispose()
        {
            RunOnDatabaseAsync(async context => await context.Database.EnsureDeletedAsync()).Wait();

            Factory.Dispose();
        }

        public void ConfigureLogging(Action<ILoggingBuilder> loggingConfiguration)
        {
            _loggingConfiguration = loggingConfiguration;
        }

        public void ConfigureServicesBeforeStartup(Action<IServiceCollection> servicesConfiguration)
        {
            _beforeServicesConfiguration = servicesConfiguration;
        }

        public void ConfigureServicesAfterStartup(Action<IServiceCollection> servicesConfiguration)
        {
            _afterServicesConfiguration = servicesConfiguration;
        }

        public async Task RunOnDatabaseAsync(Func<TDbContext, Task> asyncAction)
        {
            using IServiceScope scope = Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

            await asyncAction(dbContext);
        }

        public async Task<(HttpResponseMessage httpResponse, TResponseDocument responseDocument)>
            ExecuteGetAsync<TResponseDocument>(string requestUrl)
        {
            return await ExecuteRequestAsync<TResponseDocument>(HttpMethod.Get, requestUrl);
        }

        public async Task<(HttpResponseMessage httpResponse, TResponseDocument responseDocument)>
            ExecutePostAsync<TResponseDocument>(string requestUrl, object requestBody)
        {
            return await ExecuteRequestAsync<TResponseDocument>(HttpMethod.Post, requestUrl, requestBody);
        }

        public async Task<(HttpResponseMessage httpResponse, TResponseDocument responseDocument)>
            ExecutePatchAsync<TResponseDocument>(string requestUrl, object requestBody)
        {
            return await ExecuteRequestAsync<TResponseDocument>(HttpMethod.Patch, requestUrl, requestBody);
        }

        public async Task<(HttpResponseMessage httpResponse, TResponseDocument responseDocument)>
            ExecuteDeleteAsync<TResponseDocument>(string requestUrl, object requestBody = null)
        {
            return await ExecuteRequestAsync<TResponseDocument>(HttpMethod.Delete, requestUrl, requestBody);
        }

        private async Task<(HttpResponseMessage httpResponse, TResponseDocument responseDocument)>
            ExecuteRequestAsync<TResponseDocument>(HttpMethod method, string requestUrl, object requestBody = null)
        {
            var request = new HttpRequestMessage(method, requestUrl);
            string requestText = SerializeRequest(requestBody);

            if (!string.IsNullOrEmpty(requestText))
            {
                request.Content = new StringContent(requestText);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue(HeaderConstants.MediaType);
            }

            using HttpClient client = Factory.CreateClient();
            HttpResponseMessage responseMessage = await client.SendAsync(request);

            string responseText = await responseMessage.Content.ReadAsStringAsync();
            var responseDocument = DeserializeResponse<TResponseDocument>(responseText);

            return (responseMessage, responseDocument);
        }

        private string SerializeRequest(object requestBody)
        {
            string requestText = requestBody is string stringRequestBody
                ? stringRequestBody
                : JsonConvert.SerializeObject(requestBody);

            return requestText;
        }

        private TResponseDocument DeserializeResponse<TResponseDocument>(string responseText)
        {
            if (typeof(TResponseDocument) == typeof(string))
            {
                return (TResponseDocument)(object)responseText;
            }

            try
            {
                return JsonConvert.DeserializeObject<TResponseDocument>(responseText);
            }
            catch (JsonException exception)
            {
                throw new FormatException($"Failed to deserialize response body to JSON:\n{responseText}", exception);
            }
        }

        private sealed class IntegrationTestWebApplicationFactory : WebApplicationFactory<EmptyStartup>
        {
            private Action<ILoggingBuilder> _loggingConfiguration;
            private Action<IServiceCollection> _beforeServicesConfiguration;
            private Action<IServiceCollection> _afterServicesConfiguration;

            public void ConfigureLogging(Action<ILoggingBuilder> loggingConfiguration)
            {
                _loggingConfiguration = loggingConfiguration;
            }

            public void ConfigureServicesBeforeStartup(Action<IServiceCollection> servicesConfiguration)
            {
                _beforeServicesConfiguration = servicesConfiguration;
            }

            public void ConfigureServicesAfterStartup(Action<IServiceCollection> servicesConfiguration)
            {
                _afterServicesConfiguration = servicesConfiguration;
            }

            protected override IHostBuilder CreateHostBuilder()
            {
                return Host.CreateDefaultBuilder(null)
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.ConfigureLogging(options =>
                        {
                            _loggingConfiguration?.Invoke(options);
                        });

                        webBuilder.ConfigureServices(services =>
                        {
                            _beforeServicesConfiguration?.Invoke(services);
                        });
                    
                        webBuilder.UseStartup<TStartup>();

                        webBuilder.ConfigureServices(services =>
                        {
                            _afterServicesConfiguration?.Invoke(services);
                        });
                    });
            }
        }
    }
}
