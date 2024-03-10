using JsonApiDotNetCore.OpenApi.Client.Kiota;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using OpenApiKiotaClientExample;
using OpenApiKiotaClientExample.GeneratedCode;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddLogging(options => options.ClearProviders());
builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<ColoredConsoleLogHttpMessageHandler>();
builder.Services.AddSingleton<SetQueryStringHttpMessageHandler>();
builder.Services.AddSingleton<IAuthenticationProvider, AnonymousAuthenticationProvider>();

// @formatter:wrap_chained_method_calls chop_always
builder.Services.AddHttpClient<ExampleApiClient>()
    .ConfigurePrimaryHttpMessageHandler(_ =>
    {
        IList<DelegatingHandler> defaultHandlers = KiotaClientFactory.CreateDefaultHandlers();
        HttpMessageHandler defaultHttpMessageHandler = KiotaClientFactory.GetDefaultHttpMessageHandler();

        // Or, if your generated client is long-lived, respond to DNS updates using:
        // HttpMessageHandler defaultHttpMessageHandler = new SocketsHttpHandler();

        return KiotaClientFactory.ChainHandlersCollectionAndGetFirstLink(defaultHttpMessageHandler, defaultHandlers.ToArray())!;
    })
    .AddHttpMessageHandler<SetQueryStringHttpMessageHandler>()
    .AddHttpMessageHandler<ColoredConsoleLogHttpMessageHandler>()
    .AddTypedClient((httpClient, serviceProvider) =>
    {
        var authenticationProvider = serviceProvider.GetRequiredService<IAuthenticationProvider>();
        var requestAdapter = new HttpClientRequestAdapter(authenticationProvider, httpClient: httpClient);
        return new ExampleApiClient(requestAdapter);
    });
// @formatter:wrap_chained_method_calls restore

IHost host = builder.Build();
await host.RunAsync();
